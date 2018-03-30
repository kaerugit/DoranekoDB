using DoranekoDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using static TemplateHelper.Program;

namespace TemplateHelper
{
    class CreateTemplate
    {
        public static string CreateSchemaToJson(string connectionstring, LANG_TYPE lt)
        {


            var db = CommonData.GetDB(connectionstring);


            //テーブルスキーマjsonの取得

            var dtSchema = db.GetDataTable(db.GetSchemaSQL());
            var deleteFieldList = new List<string>();

            Type t = typeof(FieldDataMember);

            foreach (System.Data.DataColumn col in dtSchema.Columns)
            {
                var findFlag = false;
                //必要なフィールド
                foreach (MemberInfo m in t.GetProperties())
                {
                    if (col.ColumnName.Equals(m.Name))
                    {
                        findFlag = true;
                        break;
                    }
                }
                if (findFlag == false)
                {
                    deleteFieldList.Add(col.ColumnName);
                }

            }
            foreach (var filedName in deleteFieldList)
            {
                dtSchema.Columns.Remove(filedName);
            }

            var schemaJson = Newtonsoft.Json.JsonConvert.SerializeObject(dtSchema);
            schemaJson = schemaJson.Replace(@"""", @"""""");

            var kaigyo = "\r\n";

            var dataString =
                "public class CommonDataInfo" + kaigyo +
                "{" + kaigyo +
                    "    public const string SchemaJsonString = @\"" + kaigyo +
                     "{0}" + kaigyo +
                    "\";" + kaigyo +
                "}" + kaigyo ;

            if (lt == LANG_TYPE.VB)
            {
                dataString =
                    "Public Class CommonDataInfo" + kaigyo +
                        "    Public Const SchemaJsonString As String = \"" + kaigyo +
                            "{0}" + kaigyo +
                        "\"" + kaigyo +
                    "End Class" + kaigyo;
            }

            //なぜかエラーになるので置換する
            //return string.Format(dataString, schemaJson) ;
            return dataString.Replace("{0}", schemaJson);
        }

        internal static string CreateDbTable(string connectionstring,  LANG_TYPE lt)
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var db = CommonData.GetDB(connectionstring);


            //テーブルスキーマjsonの取得
            DataTable dt = db.GetDataTable(db.GetSchemaSQL());
            List<string> tableList = new List<string>();
            string tab = "    ";        //\t
            string kaigyo = "\r\n";

            string publicClassStart = "public class DbTable" + kaigyo + "{ ";
            string publicClassEnd = "}";
            string classStart = "public class {0}" + kaigyo ;
            string classStartKigo = "{";
            string classEnd = "}";
            string nameModule = "public const string Name = \"{0}\";";
            string primaryKeyModule = "public const string PrimaryKey = \"{0}\";";
            string fullNameModule = "public const string FullName = \"{0}\";";
            string commentModule = "/// <summary>{0}</summary>" + kaigyo +
                        "/// <remarks>" + kaigyo +
                        "{1}" +
                        "/// </remarks>" + kaigyo;


            if  (lt == LANG_TYPE.VB) { 
                 publicClassStart = "Public Class DbTable";
                 publicClassEnd = "End Class";
                 classStart = "Class {0}";
                 classStartKigo = "";
                 classEnd = "End Class";
                 nameModule = "Public Const Name As String = \"{0}\"";
                 primaryKeyModule = "Public Const PrimaryKey As String = \"{0}\"";
                 fullNameModule = "Public Const FullName As String = \"{0}\"";
                 commentModule = "\'\'\' <summary>{0}</summary>"+ kaigyo + 
                            "\'\'\' <remarks>" + kaigyo + 
                            "{1}" + 
                            "\'\'\' </remarks>" + kaigyo;
            }
            string comment = commentModule.Substring(0, 3);
            foreach (DataRow dr in dt.Rows)
            {
                if ((tableList.Contains(dr["TABLE_NAME"].ToString()) == false))
                {
                    tableList.Add(dr["TABLE_NAME"].ToString());
                }

            }

            Dictionary<string, string> fieldDictionary = new Dictionary<string, string>();
            foreach (string tableName in tableList)
            {
                DataRow[] drSelect = dt.Select("TABLE_NAME=\'"  + tableName + "\'");
                string primarykey = "";
                foreach (DataRow dr in drSelect)
                {
                    if (dr["IS_PRIMARYKEY"].ToString().Equals(DBMastar.DBTrueValue.ToString()))
                    {
                        primarykey += "," + dr["COLUMN_NAME"].ToString();
                    }

                }

                if ((string.IsNullOrEmpty(primarykey) == false))
                {
                    primarykey = primarykey.Substring(1);
                }

                string logicalname = tableName;
                string description = "";
                if ((dt.Columns.Contains("LOGICAL_TABLE_NAME") == true))
                {
                    if ((string.IsNullOrEmpty(drSelect[0]["LOGICAL_TABLE_NAME"].ToString()) == false))
                    {
                        logicalname = drSelect[0]["LOGICAL_TABLE_NAME"].ToString();
                    }

                }

                if ((dt.Columns.Contains("TABLE_DESCRIPTION") == true))
                {
                    if ((string.IsNullOrEmpty(drSelect[0]["TABLE_DESCRIPTION"].ToString()) == false))
                    {
                        description = drSelect[0]["TABLE_DESCRIPTION"].ToString();
                        description = (comment + description.Replace(kaigyo, (kaigyo + comment)));
                        if ((description.EndsWith(kaigyo) == false))
                        {
                            description += kaigyo;
                        }

                    }

                }

                string xmlComment = string.Format(commentModule, logicalname, description);
                sb.Append(xmlComment.Replace(comment, (tab + comment)));    // コメント

                //sb.AppendLine(tab + string.Format(classStart, tableName));
                sb.AppendLine(tab + classStart.Replace("{0}", tableName));
                if (classStartKigo.Equals("") == false)
                {
                    sb.AppendLine(tab + classStartKigo);
                }
                sb.Append(xmlComment.Replace(comment, tab + tab + comment)); //'コメント(nameのところにも同じコメント)
                
                sb.AppendLine(tab + tab + string.Format(nameModule, tableName));
                sb.AppendLine(tab + tab + string.Format(primaryKeyModule, primarykey));
                // フィールド
                foreach (DataRow dr in drSelect)    
                {
                    string columnName = dr["COLUMN_NAME"].ToString();
                    if ((fieldDictionary.ContainsKey(columnName) == true))
                    {
                        if (fieldDictionary[columnName].Equals(dr["DATA_TYPE"].ToString()) == false)
                        {
                            //同じフィールド名で フィールド：varchar    フィールド:int と複数存在する場合エラー　　　　varchar(40)、varchar(100)　などもエラーとする
                            throw (new ApplicationException(columnName + "で、Datatypeが統一されていません。" + kaigyo + columnName + "の型は全て同じにしてください"));

                        }
                        else
                        {
                            fieldDictionary[columnName] = dr["DATA_TYPE"].ToString();
                        }

                    }

                    logicalname = columnName;
                    description = "";
                    if ((dt.Columns.Contains("LOGICAL_COLUMN_NAME") == true))
                    {
                        if ((string.IsNullOrEmpty(dr["LOGICAL_COLUMN_NAME"].ToString()) == false))
                        {
                            logicalname = dr["LOGICAL_COLUMN_NAME"].ToString();
                        }

                    }

                    if ((dt.Columns.Contains("COLUMN_DESCRIPTION") == true))
                    {
                        if ((string.IsNullOrEmpty(dr["COLUMN_DESCRIPTION"].ToString()) == false))
                        {
                            description = dr["COLUMN_DESCRIPTION"].ToString();
                            description = comment + description.Replace(kaigyo, kaigyo + comment);
                            if ((description.EndsWith(kaigyo) == false))
                            {
                                description += kaigyo;
                            }

                        }

                    }

                    xmlComment = string.Format(commentModule, logicalname, description);

                    sb.Append(xmlComment.Replace(comment, tab + tab + comment));    //コメント

                    //sb.AppendLine(tab + tab + string.Format(classStart, columnName));
                    sb.AppendLine(tab + tab + classStart.Replace("{0}", columnName));
                    if (classStartKigo.Equals("") == false)
                    {
                        sb.AppendLine(tab + tab + classStartKigo);
                    }

                    sb.Append(xmlComment.Replace(comment, tab + tab + tab + comment));  // コメント(nameのところにも同じコメント)
                    sb.AppendLine(tab + tab + tab + string.Format(nameModule, columnName));

                    sb.Append(xmlComment.Replace(comment, tab + tab + tab + comment));  // コメント(fullNameのところにも同じコメント)
                    sb.AppendLine(tab + tab + tab + string.Format(fullNameModule, tableName + "." + columnName));
                    sb.AppendLine(tab + tab + classEnd);
                }
                sb.AppendLine(tab + classEnd);
            }
            return
                publicClassStart + kaigyo +
                    sb.ToString() +
                publicClassEnd;
        }
    }
}
