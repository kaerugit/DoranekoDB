using DoranekoDB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

//メモ　開始するには　メニュー　テストー実行(or デバッグ)-全てのテスト

namespace SampleAndTest
{


    #region "順番用　https://stackoverflow.com/questions/9210281/how-to-set-the-test-case-sequence-in-xunit　参考"

    [AttributeUsage(AttributeTargets.Method)]
    public class TestPriorityAttribute : Attribute
    {
        public TestPriorityAttribute(int priority)
        {
            this.Priority = priority;
        }

        public int Priority { get; }
    }

    public class PriorityOrderer : ITestCaseOrderer
    {

        //[TestCaseOrderer("SampleAndTest.PriorityOrderer", "SampleAndTest")]　の個所を定義

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedMethods = new Dictionary<int, TTestCase>();

            foreach (var testCase in testCases)
            {
                var attributeInfo = testCase.TestMethod.Method.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
                    .SingleOrDefault();
                if (attributeInfo != null)
                {
                    var priority = attributeInfo.GetNamedArgument<int>("Priority");
                    sortedMethods.Add(priority, testCase);
                }
            }

            return sortedMethods.OrderBy(x => x.Key).Select(x => x.Value);
        }
    }
    #endregion

    [TestCaseOrderer("SampleAndTest.PriorityOrderer", "SampleAndTest")] //PriorityOrderer
    public class UnitTest1
    {
        enum DATA
        {
            one = 1
        }

        public UnitTest1()
        {

            //必ず1回実行
            CommonData.InitDB();

        }

        [Fact(DisplayName = "01データベース初期設定"), TestPriority(1)]
        public void DBInit()
        {
            //CommonData.InitDB();

            Assert.NotEqual(0, (long)DBFieldData.FieldDataMemberList.Count);
        }

        [Fact(DisplayName = "02データ追加・更新(SQL)"), TestPriority(2)]
        public void DBSQL()
        {
            var db = CommonData.GetDB();

            //最大の番号を取得
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.テスト用番号.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            db.BeginTransaction();

            //テスト用データの挿入
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //好みで・・
            {
                //■■通常の挿入　SQL文を利用しての挿入■■　ごちゃごちゃやってますが、結論　db.Execute(　で実行されるsql文を確認するのが早いと思います・・

                //SQLのイメージ
                //tbl.Field のキー insert into テーブル名(★)
                //tbl.Field の値   select ★

                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト" + System.DateTime.Now.ToString());
                //↓↑同じ意味
                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト");
                //tbl[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "テスト");

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.テスト用番号.Name, maxData);
                tbl.AddParameter(DbTable.T_TEST.フラグ.Name, true);

                tbl.AddParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one);

                //nvarchar(50)に51文字入れた場合のテスト（こちらのDBクラスでは入らない文字は区切ってセット）⇒"あ"が欠落
                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "12345678901234567890123456789012345678901234567890あ");
                //数値欄がオーバーした場合のテスト（こちらのDBクラスではnull(or デフォルト値)をセット）
                tbl.AddParameter(DbTable.T_TEST.金額.Name, double.MaxValue);

                //DbTableに定義されていないフィールドもOK  (フィールドタイプは、何かを拝借すること)
                //tbl.Field["てすと"] = db.AddParameter(DbTable.T_TEST.フラグ.Name, "aaaa");

                //(InsertIntoSQLプロパティを使えば)更新日時はCommonData.GetDB(InsertUpdateDataParameter)内で自動的に更新
                db.Execute($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        values 
                                ({tbl.InsertSelectSQL}) 
                    ");

            }

            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field))
            {
                Assert.True(tbl.Field.Keys.Count != 0);

                //■■SQL文を利用しての挿入■■
                //tbl.Field　には Autonumber（自動挿入off）を除いたデータがセットされています。(取得したい場合は　TableHelperの引数参照)
                var oldmaxkbn = maxData;

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.テスト用番号.Name, maxData);
                //sql文(substringの記述)も可能
                tbl[DbTable.T_TEST.TEXTDATA.Name] = "substring(" + DbTable.T_TEST.TEXTDATA.Name + ",1,10)";     //tbl.Field[] の省略形
                tbl[DbTable.T_TEST.金額.Name] = "1000";

                //他設定されていないデータは元データの値をinsert
                db.Execute($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, oldmaxkbn)}

                    ");


                //■■同じようなテーブルにコピー（T_TESTのデータにあってT_TEST_COPYにないデータをコピー）■■
                tbl.FieldGetType = TableHelper.FIELD_GET_TYPE.FieldAndTable;
                tbl.Reset();

                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] =  "T_TEST.TEXTDATA" のデータが入っています。
                //テーブル名が必要な時は　FullName　を使用
                tbl[DbTable.T_TEST.ID_AUTO.Name] = DbTable.T_TEST.ID_AUTO.FullName;   //T_TEST_COPYのID_AUTOはオートナンバーではない
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, System.DBNull.Value);

                var sql =
                    $@"
                        insert into {DbTable.T_TEST_COPY.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                                left join   {DbTable.T_TEST_COPY.Name}
                                    on {DbTable.T_TEST.ID_AUTO.FullName} = {DbTable.T_TEST_COPY.ID_AUTO.FullName} 
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST_COPY.ID_AUTO.FullName, System.DBNull.Value)}

                    ";

                db.Execute(sql);
            }


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

            db = CommonData.GetDB();
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //好みで・・
            {
                //■■Updateテスト■■
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, "TEST");

                //直接SQL文を書くことも可能
                tbl.Field[DbTable.T_TEST.金額.Name] = $@"
                            case when {db.AddWhereParameter(DbTable.T_TEST.金額.Name, 0, "<>")} then
                                {DbTable.T_TEST.金額.Name} + 100
                            else
                                {DbTable.T_TEST.金額.Name}
                            end
                            "
                     ;
                //(UpdateSetSQLを使えば)更新日時なども自動でセット
                db.Execute($@"
                        update {DbTable.T_TEST_COPY.Name}
                        set 
                                {tbl.UpdateSetSQL}
                    ");
            }

            db.Close();

        }


        [Fact(DisplayName = "03通常のSELECT"), TestPriority(3)]
        public void Select1()
        {

            //※01から順番で実行しないとエラーになるので注意

            var db = CommonData.GetDB();


            //最大の番号を取得
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.テスト用番号.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            //通常のwhere
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, maxData)}
                    
               ");

            if (dt.Rows.Count > 0)
            {
                //一旦エラーになるが、実行順番の関係で2回目以降は実行される（はず）
                Assert.Equal(1, (long)dt.Rows.Count);
            }



            //■■in句■■
            var lst = new List<int>() { maxData, maxData - 1 };
            //AddWhereParameter の場所で　テスト用番号 in (xx,yy) のデータが作成
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}  
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, lst)}
                    
               ");
            Assert.True(dt.Rows.Count > 1);


            //■■enumのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //■■以上■■
            //テスト用番号>=xx 
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, maxData, DBMastar.WHERE_FUGO.Ijyo)}
                    
               ");

            Assert.Equal(1, dt.Rows.Count);


            //■■Likeのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "1234", DBMastar.WHERE_FUGO.Like)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //■■フラグのテスト■■
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.フラグ.Name, null)}
               ");

            Assert.True(dt.Rows.Count > 1);

        }


        [Fact(DisplayName = "04通常のSELECT(sql文の使いまわし)"), TestPriority(4)]
        public void Select2()
        {

            var db = CommonData.GetDB();

            var sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 0, DBMastar.WHERE_FUGO.Not)}
            ";


            var count1 = db.GetDataTable(sql).Rows.Count;

            //元のsql文(変数sql)を使いまわし
            var sqlSub = $@"
                    select 
                    from 
                    ({sql}) as sub
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

            var count2 = db.GetDataTable(sql).Rows.Count;

            Assert.Equal(count1, count2);


            //レスポンスNG例
            for (var i = 0; i < 100; i++)
            {
                sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

                //こうすると常に内部パラメータが溜まるのでレスポンス的にまずくなってくる
                var dt= db.GetDataTable(sql);

            }

            //レスポンスOK例　↑の正しい書き方
            for (var i = 0; i < 100; i++)
            {

                //★一旦こちらを実行（内部パラメータをクリア） 入れる事によって早くなります。
                db.ClearSQLParameter();

                sql = $@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        { db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 0, DBMastar.WHERE_FUGO.Not)}
                ";

                var dt = db.GetDataTable(sql);

            }


        }

        const int MAX_DATA = 10000;

        [Fact(DisplayName = "05データ追加・更新(Dataset)"), TestPriority(5)]
        public void DBDataSet()
        {
            var db = CommonData.GetDB();
            db.ConnectionAutoClose = false;

            db.BeginTransaction();

            //■■新規追加■■
            db.OpenDataSet($@"
                    select 
                        * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.ID_AUTO.Name, System.DBNull.Value)}
                ");


            var dt = db.DataSet.Tables[0];
            var nowString = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            //100件データを作成
            for (var i = 0; i < MAX_DATA; i++)
            {
                var drNew = dt.NewRow();

                drNew[DbTable.T_TEST.TEXTMAX.Name] = nowString;
                drNew[DbTable.T_TEST.TEXTDATA.Name] = "挿入" + (i + 1).ToString();
                drNew[DbTable.T_TEST.金額.Name] = i * 1000;
                dt.Rows.Add(drNew);
            }

            //更新(更新日時は CommonData.GetDB InsertUpdateDataDataRow 内で処理)
            db.UpdateDataSet();

            //■■更新■■
            db.OpenDataSet($@"
                    select 
                         * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "挿入", DBMastar.WHERE_FUGO.Like)}
                ", "なまえつき");


            dt = db.DataSet.Tables["なまえつき"];


            foreach (System.Data.DataRow dr in dt.Rows)
            {


                dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "更新";
                //dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "更新";
                dr[DbTable.T_TEST.金額.Name] = decimal.Parse(dr[DbTable.T_TEST.金額.Name].ToString()) + 100;
            }

            //更新（引数指定で　対象テーブルのみ更新も可）
            db.UpdateDataSet("なまえつき");


            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }


            //
            var sql = $@"
                    select 
                         * 
                        ,{DbTable.T_TEST.フラグ.Name} as 定義フィールドを勝手に追加
                    from 
                        {DbTable.T_TEST.Name}
                    where
                             {db.AddWhereParameter(DbTable.T_TEST.金額.Name, 0, ">=")}
                        and  {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, nowString)}
                ";

            var dtSelect = db.GetDataTable(sql);

            //最後のデータを削除
            dtSelect.Rows.Remove(dtSelect.Rows[dtSelect.Rows.Count - 1]);

            //データを変更
            var drSelect = dtSelect.NewRow();
            drSelect[DbTable.T_TEST.TEXTDATA.Name] = "ついかでーた";
            drSelect[DbTable.T_TEST.金額.Name] = 10;

            if (System.IO.File.Exists(@"C:\inetpub\wwwroot\iis-85.png") == true)
            {
                FileStream fs = new FileStream(@"C:\inetpub\wwwroot\iis-85.png", FileMode.OpenOrCreate, FileAccess.Read);
                byte[] MyData = new byte[fs.Length];
                fs.Read(MyData, 0, System.Convert.ToInt32(fs.Length));

                fs.Close();

                drSelect[DbTable.T_TEST.バイナリー.Name] = MyData;
            }
            drSelect[DbTable.T_TEST.フラグ.Name] = DBMastar.DBTrueValue;

            //true false(bool)の変換 ※nullの場合エラーになるのでプロジェクト用に専用関数作ってもよいかも・・
            Assert.True((bool)(drSelect[DbTable.T_TEST.フラグ.Name]));                     //変換かけて true判定でもOK(ただしnullの場合エラー)
            Assert.True(drSelect.Field<bool>(DbTable.T_TEST.フラグ.Name));                 //nget より　System.Data.DataSetExtensions　の参照が必要　書き方がちょっとわかりにくい？？
            //Assert.Equal(drSelect[DbTable.T_TEST.フラグ.Name],DBMastar.DBTrueValue);       //こちらでもOK

            drSelect["定義フィールドを勝手に追加"] = DBMastar.DBTrueValue;
            dtSelect.Rows.Add(drSelect);


            dtSelect.Rows[0][DbTable.T_TEST.入力日.Name] = System.DateTime.Now.Date;
            dtSelect.Rows[0][DbTable.T_TEST.金額.Name] = 888;


            //jsonに変換(web apiなどで送信)
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dtSelect);


            //jsonからDataTableに変換(web apiなどで受信)

            //jsonからの戻し(SQLServer2016 だと json をそのままtable化できるのでそちらを使用したほうがよいと思う　hint OPENJSON関数 )
            var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            var tempTableName = "##temp" + nowString;

            if (dataList.Count > 0)
            {
                var dummyFieldChange = new Dictionary<string, string>()
                        {
                            { "定義フィールドを勝手に追加", DbTable.T_TEST.フラグ.Name }
                        };


                var tbl = new TableHelper(db, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);
                //現在のデータからCreatetableを行う
                //ToListするには　using System.Linq;　が必要
                db.Execute(tbl.GetCreateSQL(tempTableName, dataList[0].Keys.ToList()));

                //～通常版
                if (false)
                {
                    //db.BeginTransaction();
                    db.ConnectionAutoClose = false; //##テーブルが消えるので・・

                    db.OpenDataSet($"select * from  {tempTableName}");

                    var dtTemp = db.DataSet.Tables[0];
                    foreach (var data in dataList)
                    {
                        var dr = dtTemp.NewRow();
                        foreach (var key in data.Keys)
                        {
                            //無視するフィールド
                            if (tbl.IgnoreFieldList.Contains(key))
                            {
                                continue;
                            }

                            if (data[key] == null)
                            {
                                dr[key] = System.DBNull.Value;
                            }
                            else
                            {
                                //バイナリ列のみ特別処理（オブジェクトをそのまま入れるとエラーになるので、byteデータとして処理）
                                if (dtTemp.Columns[key].DataType == typeof(System.Byte[]))
                                {
                                    dr[key] = System.Convert.FromBase64String(data[key].ToString());
                                }
                                else
                                {
                                    dr[key] = data[key];
                                }
                            }
                        }
                        dtTemp.Rows.Add(dr);
                    }

                    db.UpdateDataSet();
                    //db.Commit();
                }


                //～高速版（だけど負荷がかかります）
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;

                    Parallel.ForEach(dataList, data =>
                    {

                        //Parallel.ForEach 内でのdbオブジェクトの使いまわしは厳禁
                        using (var dbpara = CommonData.GetDB())
                        {
                            var tblpara = new TableHelper(dbpara, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);

                            sql = $@"
                                insert into {tempTableName}
                                    ({filed})
                                select 
                                    {filed}
                                from 
                                    (
                                    {tblpara.GetSQL(data)}
                                    ) as ins
                                ";

                            dbpara.Execute(sql);
                        }
                    });
                }



                //こちらでもOK
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;
                    foreach (var data in dataList)
                    {

                        db.ClearSQLParameter();   //ただしこちらを実行しないとパラメータが溜まってすごく遅い

                        sql = $@"
                            insert into {tempTableName}
                                ({filed})
                            select 
                                {filed}
                            from 
                                (
                                {tbl.GetSQL(data)}
                                ) as ins
                           ";

                        db.Execute(sql);

                    }
                }

                //疑似BULK INSERT (あまり早くない)
                if (true)
                {
                    var insertCount = 200;
                    //var data = Enumerable.Range(0, dataList.Count);

                    var tblTemp = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field);
                    
                    Enumerable.Range(0, (dataList.Count / insertCount) + (dataList.Count % insertCount == 0 ? 0 : 1))
                                .Select(
                                    (index) =>
                                    {
                                        return dataList.Skip(insertCount * index).Take(insertCount).ToList();
                                    }
                               ).ToList()
                               .ForEach(
                                    (eachData) =>
                                    {
                                        var sb = new StringBuilder();
                                        sb.Append($"insert into {tempTableName}");
                                        sb.Append($"({tblTemp.InsertIntoSQL})");

                                        sb.Append($" values ");

                                        var firstFlag = true;

                                        foreach (var data in eachData)
                                        {
                                            tblTemp.Reset();

                                            foreach (var eachDic in tblTemp.Field.ToList())
                                            {
                                                object value  = System.DBNull.Value;
                                                if (data.Keys.Contains(eachDic.Key))
                                                {
                                                    value = data[eachDic.Key];
                                                }

                                                tblTemp.AddParameter(eachDic.Key, value);
                                            }

                                            if (firstFlag == true)
                                            {
                                                firstFlag = false;
                                            }
                                            else
                                            {
                                                sb.Append($",");
                                            }

                                            sb.Append($"({tblTemp.InsertSelectSQL})");

                                        }

                                        db.Execute(sb.ToString());
                                        db.ClearSQLParameter();
                                    }
                                );

                }

            }


            //更新
            db.BeginTransaction();
            var mergeSql = SQLHelper.GetMergeSQL(db, DbTable.T_TEST.Name, $" select * from {tempTableName}");
            db.Execute(mergeSql);
            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

            db.Close();
        }

        [Fact(DisplayName = "06パラメータの保存"), TestPriority(6)]
        public void SaveParameter()
        {
            var db = CommonData.GetDB();

            //パラメータの保存
            db.ClearSQLParameter();

            var lst = new List<int>() { 100, 200 };

            var sql = $@"
                 select 
                   * 
                 from
                    {DbTable.T_TEST.Name} 
                 where
                        {db.AddWhereParameter(DbTable.T_TEST.金額.Name, lst, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.金額.Name, DATA.one, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.金額.Name, System.DBNull.Value, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.フラグ.Name, false, DBMastar.WHERE_FUGO.Not)}
            ";

            var datacount = db.GetDataTable(sql).Rows.Count;

            //jsonオブジェクトに変換
            var save = Newtonsoft.Json.JsonConvert.SerializeObject(db.SQLParameter);

            //違うオブジェクトに復元
            var db2 = CommonData.GetDB();

            //戻す（この場合System.DBNull.Value ⇒ nullに変換されるのがDBMastarクラスで対応済）
            db2.SQLParameter = Newtonsoft.Json.JsonConvert.DeserializeObject<DBSQLParameter>(save);
            var dt2 = db2.GetDataTable(sql);

            Assert.Equal(datacount, dt2.Rows.Count);

        }

        [Fact(DisplayName = "07パラメータのコピー"), TestPriority(7)]
        public void CopyParameter()
        {

            //条件を保存
            var dbSave = CommonData.GetDB();
            var whereSave = $@"
                    where
                        { dbSave.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 1, DBMastar.WHERE_FUGO.Ijyo)}
                    ";

            var db1 = CommonData.GetDB();
            //保存したパラメータをコピー(dbSave→db1)
            var where1 = db1.CopySQL(dbSave, whereSave);     //戻り値は whereSave

            var dt1 = db1.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    {where1}
               ");


            var db2 = CommonData.GetDB();
            var where2 = db2.CopySQL(dbSave, whereSave);     //戻り値は whereSave

            var dt2 = db2.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    {where2}
               ");

            Assert.Equal(dt1.Rows.Count, dt2.Rows.Count);

        }



    }


}
