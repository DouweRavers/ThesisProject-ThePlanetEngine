using UnityEngine;

namespace PlanetEngine {
	public class LargeVector3 {

		public static LargeVector3 operator +(LargeVector3 a, LargeVector3 b) {
			return new LargeVector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}
		public static LargeVector3 operator -(LargeVector3 a, LargeVector3 b) {
			return new LargeVector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}
		public static LargeVector3 operator +(LargeVector3 a, Vector3 b) {
			return new LargeVector3(a.x + (decimal)b.x, a.y + (decimal)b.y, a.z + (decimal)b.z);
		}
		public static LargeVector3 operator -(LargeVector3 a, Vector3 b) {
			return new LargeVector3(a.x - (decimal)b.x, a.y - (decimal)b.y, a.z - (decimal)b.z);
		}
		public override string ToString() => $"{x}, {y}, {z}";


		public static LargeVector3 zero { get { return new LargeVector3(); } }
		public decimal x = 0, y = 0, z = 0;
		public LargeVector3(decimal x = 0, decimal y = 0, decimal z = 0) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public Vector3 toVector3() {
			return new Vector3((float)x, (float)y, (float)z);
		}
	}
}