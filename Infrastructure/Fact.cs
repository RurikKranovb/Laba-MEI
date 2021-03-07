using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Laba2.Runner.Utils;

namespace Laba2.Runner.Infrastructure
{
	public class Fact : BindableBase
	{
		private int _id;
		private string _object;
		private string _attribute;
		private string _value;
		private double _truth;
		private FactType _type;
        private bool _isSelected;


        public bool IsSelected
		{
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

		public int ID
		{
			get => _id;
            set => SetProperty(ref _id, value);
		}

        public string Object
		{
			get => _object;
            set => SetProperty(ref _object, value);
		}

        public string Attribute
		{
			get => _attribute;
            set => SetProperty(ref _attribute, value);
		}

        public string Value
		{
			get => _value;
            set => SetProperty(ref _value, value);
		}

        public double Truth
		{
			get => _truth;
            set => SetProperty(ref _truth, value);
		}

        public FactType Type
		{
			get => _type;
            set => SetProperty(ref _type, value);
		}

        override public string ToString()
		{
			return Object + "__" + Attribute + "__" + Value;
		}

		public void WriteToFile(BinaryWriter bw)
		{
			System.Text.Encoding win1251Enc = System.Text.Encoding.GetEncoding("windows-1251");

			bw.Write(_id);
			bw.Write(win1251Enc.GetByteCount(_object));
			bw.Write(win1251Enc.GetBytes(_object));
			bw.Write(win1251Enc.GetByteCount(_attribute));
			bw.Write(win1251Enc.GetBytes(_attribute));
			bw.Write(win1251Enc.GetByteCount(_value));
			bw.Write(win1251Enc.GetBytes(_value));
			bw.Write(_truth);
			bw.Write((int)_type);
		}

		public Fact ReadFromFile(BinaryReader br)
		{
			System.Text.Encoding win1251Enc = System.Text.CodePagesEncodingProvider.Instance.GetEncoding("windows-1251");
			Byte[] buf = null;

			_id = br.ReadInt32();

			// _object
			int strLen = br.ReadInt32();
			if (strLen > 0 && strLen < 1024)
			{
				buf = new Byte[strLen];
				br.Read(buf, 0, buf.Length);

				_object = win1251Enc.GetString(buf, 0, buf.Length);
			}

			// _attribute
			strLen = br.ReadInt32();
			if (strLen > 0 && strLen < 1024)
			{
				buf = new Byte[strLen];
				br.Read(buf, 0, buf.Length);

				_attribute = win1251Enc.GetString(buf, 0, buf.Length);
			}

			// _value
			strLen = br.ReadInt32();
			if (strLen > 0 && strLen < 1024)
			{
				buf = new Byte[strLen];
				br.Read(buf, 0, buf.Length);

				_value = win1251Enc.GetString(buf, 0, buf.Length);
			}

			_truth = br.ReadDouble();
			_type = (FactType)br.ReadInt32();

            return this;
        }
	}
}
