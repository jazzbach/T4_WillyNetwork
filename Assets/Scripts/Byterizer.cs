using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace LLNet {

	public class Byterizer {
		private byte[] _Buffer;
		private int _Index = 0;

		public Byterizer() {
			ResetBuffer();
		}

		public void ResetIndex() {
			_Index = 0;
		}

		public void ResetBuffer() {
			_Buffer = new byte[0];
			_Index = 0;
		}

		public byte[] GetBuffer() {
			return _Buffer;
		}

		public void LoadDeep(byte[] data, int length = -1) {
			if (length == -1) { length = data.Length; }
			ResetBuffer();
			if (length == 0) return;
			_Buffer = new byte[length];
			Buffer.BlockCopy(data, 0, _Buffer, 0, length);
		}

		public void LoadShallow(byte[] data) {
			ResetBuffer();
			_Buffer = data;
		}


		public void Push(bool val) { _Buffer = ConcatByteArrays(_Buffer, new byte[] { (byte)(val == true ? 1 : 0) }); }
		public void Push(byte val) { _Buffer = ConcatByteArrays(_Buffer, new byte[] { val }); }
		public void Push(sbyte val) { _Buffer = ConcatByteArrays(_Buffer, new byte[] { (byte)val }); }
		public void Push(Int16 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(Int32 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(Int64 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(UInt16 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(UInt32 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(UInt64 val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(float val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(double val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(char val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val)); }
		public void Push(DateTime val) { _Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(val.Ticks)); }
		public void Push(string val) {
			byte[] barr = Encoding.Unicode.GetBytes(val);
			_Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(barr.Length), barr);
		}
		public void PushB64(string val) {
			byte[] barr = Encoding.UTF8.GetBytes(val);
			string b64 = Convert.ToBase64String(barr);
			barr = Encoding.Unicode.GetBytes(b64);
			_Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(barr.Length), barr);
		}
		public void PushB64(string[] arr) {
			Push(arr.Length);
			for (int i = 0; i < arr.Length; ++i) {
				PushB64(arr[i]);
			}
		}
		public void Push(Vector2 v2) {
			Push(v2.x);
			Push(v2.y);
		}
		public void Push(Vector3 v3) {
			Push(v3.x);
			Push(v3.y);
			Push(v3.z);
		}
		public void Push(Quaternion quat) {
			Push(quat.x);
			Push(quat.y);
			Push(quat.z);
			Push(quat.w);
		}
		public void PushSerializable(object obj) {
			if (obj == null) {
				return;
			}
			if (!obj.GetType().IsSerializable) {
				throw new Exception("Object [" + obj.GetType().Name + "] should be serializable!");
			}
			BinaryFormatter bf = new BinaryFormatter();
			MemoryStream ms = new MemoryStream();
			bf.Serialize(ms, obj);
			byte[] barr = ms.ToArray();
			_Buffer = ConcatByteArrays(_Buffer, BitConverter.GetBytes(barr.Length), barr);
		} //PushSerializable


		public bool PopBool() {
			bool val = _Buffer[_Index] == 1 ? true : false;
			++_Index;
			return val;
		}
		public byte PopByte() {
			byte val = _Buffer[_Index];
			++_Index;
			return val;
		}
		public sbyte PopSByte() {
			sbyte val = (sbyte)_Buffer[_Index];
			++_Index;
			return val;
		}
		public Int16 PopInt16() {
			Int16 val = BitConverter.ToInt16(_Buffer, _Index);
			_Index += 2;
			return val;
		}
		public Int32 PopInt32() {
			Int32 val = BitConverter.ToInt32(_Buffer, _Index);
			_Index += 4;
			return val;
		}
		public Int64 PopInt64() {
			Int64 val = BitConverter.ToInt64(_Buffer, _Index);
			_Index += 8;
			return val;
		}
		public UInt16 PopUInt16() {
			UInt16 val = BitConverter.ToUInt16(_Buffer, _Index);
			_Index += 2;
			return val;
		}
		public UInt32 PopUInt32() {
			UInt32 val = BitConverter.ToUInt32(_Buffer, _Index);
			_Index += 4;
			return val;
		}
		public UInt64 PopUInt64() {
			UInt64 val = BitConverter.ToUInt64(_Buffer, _Index);
			_Index += 8;
			return val;
		}
		public float PopFloat() {
			float val = BitConverter.ToSingle(_Buffer, _Index);
			_Index += sizeof(float);
			return val;
		}
		public double PopDouble() {
			double val = BitConverter.ToDouble(_Buffer, _Index);
			_Index += sizeof(double); ;
			return val;
		}
		public char PopChar() {
			char val = BitConverter.ToChar(_Buffer, _Index);
			_Index += 2;
			return val;
		}
		public DateTime PopDatetime() {
			DateTime val = DateTime.FromBinary(BitConverter.ToInt64(_Buffer, _Index));
			_Index += 8;
			return val;
		}
		public string PopString() {
			int len = PopInt32();
			string val = Encoding.Unicode.GetString(_Buffer, _Index, len);
			_Index += len;
			return val;
		}
		public string PopStringB64() {
			int len = PopInt32();
			string b64 = Encoding.UTF8.GetString(_Buffer, _Index, len);
			byte[] barr = Convert.FromBase64String(b64);
			_Index += len;
			return Encoding.Unicode.GetString(barr, 0, barr.Length); ;
		}
		public Vector2 PopVector2() {
			float x = PopFloat();
			float y = PopFloat();
			return new Vector2(x, y);
		}
		public Vector3 PopVector3() {
			float x = PopFloat();
			float y = PopFloat();
			float z = PopFloat();
			return new Vector3(x, y, z);
		}
		public Quaternion PopQuaternion() {
			float x = PopFloat();
			float y = PopFloat();
			float z = PopFloat();
			float w = PopFloat();
			return new Quaternion(x, y, z, w);
		}
		public object PopSerializable() {
			int len = PopInt32();
			MemoryStream memStream = new MemoryStream();
			BinaryFormatter binForm = new BinaryFormatter();
			memStream.Write(_Buffer, _Index, len);
			memStream.Seek(0, SeekOrigin.Begin);
			object obj;
			try { obj = (object)binForm.Deserialize(memStream); } catch (Exception) {
				Console.WriteLine("ByteArrayToObject Error @ index " + _Index);
				return false;
			}
			_Index += len;
			return obj;
		}


		public static byte[] ConcatByteArrays(byte[] first, byte[] second) {
			byte[] ret = new byte[first.Length + second.Length];
			Buffer.BlockCopy(first, 0, ret, 0, first.Length);
			Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
			return ret;
		}
		public static byte[] ConcatByteArrays(byte[] first, byte[] second, byte[] third) {
			byte[] ret = new byte[first.Length + second.Length + third.Length];
			Buffer.BlockCopy(first, 0, ret, 0, first.Length);
			Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
			Buffer.BlockCopy(third, 0, ret, first.Length + second.Length, third.Length);
			return ret;
		}
		public static byte[] ConcatByteArrays(params byte[][] arrays) {
			byte[] ret = new byte[arrays.Sum(x => x.Length)];
			int offset = 0;
			for (int i = 0; i < arrays.Length; i++) {
				Buffer.BlockCopy(arrays[i], 0, ret, offset, arrays[i].Length);
				offset += arrays[i].Length;
			}
			return ret;
		} //ConcatByteArrays

	}


}
