using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Laba2.Runner.Utils;

namespace Laba2.Runner.Infrastructure
{
	public class Rule : BindableBase
	{
		private int _id;
		private string _name;
		private string _condition;
		private ArrayList _conclusions;
		private double _truth;

        public Rule()
        {
            _conclusions = new ArrayList();
        }

		public int ID
		{
            get => _id;
            set => SetProperty(ref _id, value);

		}

		public string Name
		{
			get => _name;
			set => SetProperty(ref _name, value);
		}

		public string Condition
		{
			get => _condition;
            set => SetProperty(ref _condition, value);
		}

		public ArrayList Conclusions
		{
			get => _conclusions;
            set => SetProperty(ref _conclusions, value);
		}

		public double Truth
		{
			get => _truth;
            set => SetProperty(ref _truth, value);
		}

		override public string ToString()
		{
			return Name;
		}

		public void WriteToFile(BinaryWriter bw)
		{
			System.Text.Encoding win1251Enc = System.Text.Encoding.GetEncoding("windows-1251");

			bw.Write(_id);
			bw.Write(win1251Enc.GetByteCount(_name));
			bw.Write(win1251Enc.GetBytes(_name));
			bw.Write(win1251Enc.GetByteCount(_condition));
			bw.Write(win1251Enc.GetBytes(_condition));

			bw.Write(_conclusions.Count);
			foreach (int ruleID in _conclusions)
			{
				bw.Write(ruleID);
			}

			bw.Write(_truth);
		}

		public Rule ReadFromFile(BinaryReader br)
		{
			System.Text.Encoding win1251Enc = System.Text.CodePagesEncodingProvider.Instance.GetEncoding("windows-1251");
			Byte[] buf = null;

			_id = br.ReadInt32();

			// _name
			int strLen = br.ReadInt32();
			if (strLen > 0 && strLen < 1024)
			{
				buf = new Byte[strLen];
				br.Read(buf, 0, buf.Length);

				_name = win1251Enc.GetString(buf, 0, buf.Length);
			}

			// _condition
			strLen = br.ReadInt32();
			if (strLen > 0 && strLen < 1024)
			{
				buf = new Byte[strLen];
				br.Read(buf, 0, buf.Length);

				_condition = win1251Enc.GetString(buf, 0, buf.Length);
			}

			// _conslusions
			_conclusions.Clear();
			int conclusionsCount = br.ReadInt32();
			for (int i = 0; i < conclusionsCount; i++)
				_conclusions.Add(br.ReadInt32());

			_truth = br.ReadDouble();

            return this;
        }
	}
}
