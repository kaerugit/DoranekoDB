using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace DoranekoDB
{
    [Serializable]
    public struct DBUseParameter
    {
        public string ParameterName;
        public DbType DbType;
        public object Value;
    }


    public class DBSQLParameter  :  Dictionary<String, DBUseParameter>

    {
        public DBSQLParameter()
        {

        }

        #region "シリアル関連残骸(結局jsonで実装)"
        /*
        /// <summary>
        /// 逆シリアル化
        /// </summary>
        public DBSQLParameter(SerializationInfo info, StreamingContext context)
        {
            var keys = "";

            try
            {
                keys = info.GetString("keys");
            }
            catch { }

            if (keys.Equals("") == false)
            {
                foreach (var key in info.GetString("keys").Split(Char.Parse("¶")))
                {
                    DBUseParameter para;

                    para.ParameterName = info.GetString(key + "_ParameterName");
                    para.DbType = (DbType)(info.GetValue(key + "_DbType", typeof(DbType)));
                    para.Value = info.GetValue(key + "_Value", typeof(object));
                    if (para.Value == null)
                    {
                        para.Value = System.DBNull.Value;
                    }

                    this[key] = para;
                }
            }
        }


        /// <summary>
        /// シリアル化
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            //全てのパラメータをクリア
            //this.Clear();
            info.AddValue("keys", string.Join("¶", this.Keys.ToList().ToArray()));
            foreach (var key in this.Keys)
            {
                info.AddValue(key + "_ParameterName", this[key].ParameterName);
                info.AddValue(key + "_DbType", this[key].DbType);
                var value = this[key].Value;
                if (value == System.DBNull.Value)
                {
                    value = null;
                }
                info.AddValue(key + "_Value", value);

            }
        }

        public override string ToString()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // バイナリ形式でシリアライズするためのフォーマッターを生成
                BinaryFormatter formatter = new BinaryFormatter();
                // コピー元のインスタンスをシリアライズ
                formatter.Serialize(stream, this);

                // メモリーストリームの現在位置を先頭に設定
                stream.Position = 0L;

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
        */
        #endregion



    }
}
