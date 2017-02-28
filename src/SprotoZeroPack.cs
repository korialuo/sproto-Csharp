using System.Linq;

namespace Sproto {

    public static class SprotoZeroPack {

        private const int GROUP_SZ = 8;

        public static byte[] Pack(byte[] data, int len = 0) {
            SprotoStream packed = new SprotoStream();
            int idx = 0;
            int i = 0;
            len = len == 0 ? data.Length : len;
            while (idx < len) {
                byte mapz = 0;
                SprotoStream group = new SprotoStream(GROUP_SZ);
                for (i = 0; i < GROUP_SZ && idx < len; ++i) {
                    if (data[idx] != 0) {
                        mapz |= (byte)((1 << i) & 0xff);
                        group.Write(data, idx, 1);
                    }
                    ++idx;
                }
                packed.WriteByte(mapz);
                packed.Write(group.Buffer, 0, group.Position);
            }
            // If it is an unsaturated group, then fill a byte of free size.
            if (i < GROUP_SZ) {
                byte fill = (byte)(GROUP_SZ - i);
                packed.WriteByte(fill);
            }
            return packed.Buffer.Take<byte>(packed.Position).ToArray();
        }

        public static byte[] Unpack(byte[] data, int len = 0) {
            SprotoStream origin = new SprotoStream();
            int idx = 0;
            len = len == 0 ? data.Length : len;
            while (idx < len) {
                int mapz = data[idx++];
                SprotoStream group = new SprotoStream(GROUP_SZ);
                byte fill = 0;
                for (int i = 0; i < GROUP_SZ && idx < len; ++i) {
                    if ((mapz & ((1 << i) & 0xff)) != 0) {
                        group[i] = data[idx++];
                    }
                }
                // To judge whether it is a unsaturated group.
                if (idx == len - 1 && data[idx] < GROUP_SZ) {
                    fill = data[idx++];
                }
                origin.Write(group.Buffer, 0, GROUP_SZ - fill);
            }
            return origin.Buffer.Take<byte>(origin.Position).ToArray();
        }
    }
}
