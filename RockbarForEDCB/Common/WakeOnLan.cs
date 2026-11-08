using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RockbarForEDCB
{
    /// <summary>
    /// WOL送信の結果
    /// </summary>
    public enum WolResult
    {
        /// <summary>送信しない設定（WOL未使用、TCPモードではない、またはMACアドレス未設定）</summary>
        Skipped,

        /// <summary>1つ以上の宛先へ送信できた</summary>
        Sent,

        /// <summary>送信対象はあったが、すべて失敗した（名前解決の失敗を含む）</summary>
        Failed
    }

    /// <summary>
    /// Wake on LAN（マジックパケット）を送るクラス
    /// MainForm や SettingForm からは SendForSettingAsync を呼べばよい
    /// </summary>
    public static class WakeOnLan
    {
        /// <summary>
        /// 設定に従ってWOLを送る（呼び出し元の入口）
        /// 1. WolTargetHost（空欄なら HostName）を名前解決し、そのIPへユニキャストで送る
        /// 2. 解決後のIPがプライベートIPv4なら、WolSubnetMask からディレクテッドブロードキャストを求めて併用する
        /// </summary>
        /// <param name="setting">接続設定</param>
        /// <returns>送らなかった / 送れた / 失敗した のいずれか</returns>
        public static async Task<WolResult> SendForSettingAsync(RockbarSetting setting)
        {
            // WOL未使用、TCPモードではない、MACアドレス未設定のときは何もしない
            if (!setting.UseWol || !setting.UseTcpIp || string.IsNullOrWhiteSpace(setting.WolMacAddress))
            {
                return WolResult.Skipped;
            }

            // MACアドレスの形式が不正なら送らない
            byte[] mac;
            if (!TryParseMacAddress(setting.WolMacAddress, out mac))
            {
                return WolResult.Failed;
            }

            // マジックパケットは1回だけ作り、ユニキャストとブロードキャストで使い回す
            byte[] packet = BuildMagicPacket(mac);
            int port = (int)setting.WolPortNumber;

            // 1. 送信先を決める（WolTargetHost が空なら HostName を使う）
            string targetText = string.IsNullOrWhiteSpace(setting.WolTargetHost)
                ? setting.HostName
                : setting.WolTargetHost;

            // ホスト名をIPアドレスに変換する（IPの文字列ならそのまま変換される。失敗なら null）
            IPAddress target = await EdcbClient.ResolveHostAsync(targetText);
            if (target == null)
            {
                return WolResult.Failed;
            }

            // ユニキャストで送る（外から使うとき、ルーターが受け取って転送する）
            bool sent = SendPacket(target, port, packet, false);

            // 2. 解決後のIPがLAN内なら、ディレクテッドブロードキャストも送る
            //    スリープ中のPCはユニキャストに応答できないことがあるため、併用する
            IPAddress broadcast;
            if (IsPrivateIPv4(target) && TryGetDirectedBroadcast(target, setting.WolSubnetMask, out broadcast))
            {
                sent |= SendPacket(broadcast, port, packet, true);
            }

            return sent ? WolResult.Sent : WolResult.Failed;
        }

        /// <summary>
        /// MACアドレス文字列を6バイトに変換する
        /// "AA:BB:CC:DD:EE:FF"、"AA-BB-CC-DD-EE-FF"、区切りなしの12桁に対応する
        /// </summary>
        /// <param name="text">MACアドレス文字列</param>
        /// <param name="macAddress">変換結果（失敗時は null）</param>
        /// <returns>変換できたら true</returns>
        public static bool TryParseMacAddress(string text, out byte[] macAddress)
        {
            macAddress = null;
            if (string.IsNullOrWhiteSpace(text)) return false;

            // 区切り文字を取り除き、12桁の16進数か確認する
            string hex = text.Trim().Replace("-", "").Replace(":", "");
            if (hex.Length != 12) return false;

            byte[] result = new byte[6];
            for (int i = 0; i < 6; i++)
            {
                // 2桁ずつ16進数として読む
                byte value;
                if (!byte.TryParse(hex.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
                {
                    return false;
                }
                result[i] = value;
            }

            macAddress = result;
            return true;
        }

        /// <summary>
        /// サブネットマスクの文字列を32ビットの数値に変換し、正しい形かを判定する
        /// 1が連続して並ぶ形のみ有効（0.0.0.0 と、途中で1が途切れた形は無効）
        /// </summary>
        /// <param name="text">サブネットマスクの文字列（例: 255.255.255.0）</param>
        /// <param name="value">変換結果の数値（失敗時は 0）</param>
        /// <returns>正しいマスクなら true</returns>
        private static bool TryParseSubnetMask(string text, out uint value)
        {
            value = 0;

            IPAddress mask;
            if (!IPAddress.TryParse(text, out mask) || mask.AddressFamily != AddressFamily.InterNetwork) return false;

            // 4バイトを、先頭（上位）から1つの32ビット数値にまとめる
            byte[] b = mask.GetAddressBytes();
            value = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];

            // 0.0.0.0 は意味のあるマスクではないため無効
            if (value == 0) return false;

            // 反転した値に1を足して2のべき乗なら、1が連続している（後ろだけが1の形）
            uint inverse = ~value;
            return (inverse & (inverse + 1)) == 0;
        }

        /// <summary>
        /// 入力欄の検証用。サブネットマスクとして正しいかを判定する
        /// 255.255.255.255 は入力として有効（送信時にはブロードキャストを省く）
        /// </summary>
        /// <param name="text">サブネットマスクの文字列</param>
        public static bool IsValidSubnetMask(string text)
        {
            uint value;
            return TryParseSubnetMask(text, out value);
        }

        /// <summary>
        /// IPアドレスとサブネットマスクから、ディレクテッドブロードキャスト（例: 192.168.1.255）を求める
        /// 255.255.255.255 の場合はネットワークを表さないため求めない（ユニキャストのみ）
        /// </summary>
        /// <param name="ip">対象のIPアドレス</param>
        /// <param name="subnetMask">サブネットマスクの文字列</param>
        /// <param name="broadcast">求めたブロードキャストアドレス（失敗時は null）</param>
        /// <returns>求められたら true</returns>
        public static bool TryGetDirectedBroadcast(IPAddress ip, string subnetMask, out IPAddress broadcast)
        {
            broadcast = null;

            if (ip == null || ip.AddressFamily != AddressFamily.InterNetwork) return false;

            uint maskValue;
            if (!TryParseSubnetMask(subnetMask, out maskValue)) return false;

            // 255.255.255.255 はブロードキャストを求めない（ユニキャストのみ）
            if (maskValue == 0xFFFFFFFF) return false;

            // ホスト部をすべて1にすると、そのネットワークのブロードキャストになる
            byte[] ipBytes = ip.GetAddressBytes();
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                // 上の位のバイトから順に、マスクの各バイトを取り出す
                byte maskByte = (byte)(maskValue >> (24 - 8 * i));
                result[i] = (byte)(ipBytes[i] | ~maskByte);
            }

            broadcast = new IPAddress(result);
            return true;
        }

        /// <summary>
        /// プライベートIPv4アドレス（10.x / 172.16〜31.x / 192.168.x / 169.254.x）かを判定する
        /// LAN内と判断できる場合だけ、ブロードキャストを併用する
        /// </summary>
        /// <param name="address">判定するIPアドレス</param>
        public static bool IsPrivateIPv4(IPAddress address)
        {
            if (address == null || address.AddressFamily != AddressFamily.InterNetwork) return false;

            byte[] b = address.GetAddressBytes();
            return b[0] == 10 ||
                   (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                   (b[0] == 192 && b[1] == 168) ||
                   (b[0] == 169 && b[1] == 254);
        }

        /// <summary>
        /// マジックパケットを作る
        /// 先頭6バイトが0xFF、その後ろにMACアドレスを16回繰り返した、合計102バイト
        /// </summary>
        /// <param name="mac">6バイトのMACアドレス</param>
        private static byte[] BuildMagicPacket(byte[] mac)
        {
            byte[] packet = new byte[6 + mac.Length * 16];

            // 先頭の6バイトを 0xFF で埋める
            for (int i = 0; i < 6; i++)
            {
                packet[i] = 0xff;
            }

            // 続けて MACアドレスを16回並べる
            for (int i = 0; i < 16; i++)
            {
                mac.CopyTo(packet, 6 + mac.Length * i);
            }

            return packet;
        }

        /// <summary>
        /// 指定した宛先へパケットを1回送る
        /// allowBroadcast が true のときだけ、ブロードキャストを許可する
        /// </summary>
        /// <param name="destination">送信先のIPアドレス（ユニキャストまたはブロードキャスト）</param>
        /// <param name="port">送信先ポート</param>
        /// <param name="packet">送信するマジックパケット</param>
        /// <param name="allowBroadcast">ブロードキャストを許可するか</param>
        /// <returns>送信できたら true</returns>
        private static bool SendPacket(IPAddress destination, int port, byte[] packet, bool allowBroadcast)
        {
            try
            {
                // 宛先と同じアドレスファミリーのソケットで送る（使い終わったら破棄する）
                using (var udp = new UdpClient(destination.AddressFamily))
                {
                    udp.EnableBroadcast = allowBroadcast;
                    udp.Send(packet, packet.Length, new IPEndPoint(destination, port));
                }
                return true;
            }
            catch (SocketException)
            {
                // ネットワーク未接続などで失敗しても、アプリは落とさない
                return false;
            }
        }
    }
}