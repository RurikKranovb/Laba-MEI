using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Laba2.Runner.Utils;

namespace Laba2.Runner.Infrastructure
{
    public class Question : BindableBase
    {
        private int _id;
        private string _text;
        private int _yesAnswerFactID;
        private int _noAnswerFactID;

        public int ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public int YesAnswerFactID
        {
            get => _yesAnswerFactID;
            set => SetProperty(ref _yesAnswerFactID, value);
        }

        public int NoAnswerFactID
        {
            get => _noAnswerFactID;
            set => SetProperty(ref _noAnswerFactID, value);
        }

        override public string ToString()
        {
            return Text;
        }

        public void WriteToFile(BinaryWriter bw)
        {
            System.Text.Encoding win1251Enc = System.Text.CodePagesEncodingProvider.Instance.GetEncoding("windows-1251");

            bw.Write(_id);
            bw.Write(win1251Enc.GetByteCount(_text));
            bw.Write(win1251Enc.GetBytes(_text));
            bw.Write(_yesAnswerFactID);
            bw.Write(_noAnswerFactID);
        }

        public Question ReadFromFile(BinaryReader br)
        {
            System.Text.Encoding win1251Enc = System.Text.CodePagesEncodingProvider.Instance.GetEncoding("windows-1251");
            Byte[] buf = null;

            _id = br.ReadInt32();

            // _text
            int strLen = br.ReadInt32();
            if (strLen > 0 && strLen < 1024)
            {
                buf = new Byte[strLen];
                br.Read(buf, 0, buf.Length);

                _text = win1251Enc.GetString(buf, 0, buf.Length);
            }

            _yesAnswerFactID = br.ReadInt32();
            _noAnswerFactID = br.ReadInt32();

            return this;
        }
    }
}
