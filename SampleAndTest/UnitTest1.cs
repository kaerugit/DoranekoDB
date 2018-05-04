using DoranekoDB;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

//�����@�J�n����ɂ́@���j���[�@�e�X�g�[���s(or �f�o�b�O)-�S�Ẵe�X�g


namespace SampleAndTest
{
    public class UnitTest1
    {
        enum DATA
        {
            one = 1
        }


        public UnitTest1()
        {
            //�K��1����s
            CommonData.InitDB();
        }

        [Fact(DisplayName = "�f�[�^�x�[�X�����ݒ�")]
        public void DBInit()
        {
            //CommonData.InitDB();

            Assert.NotEqual(DBFieldData.FieldDataMemberList.Count, 0);
        }

        [Fact(DisplayName = "�f�[�^�ǉ��E�X�V(SQL)")]
        public void DBSQL()
        {
            var db = CommonData.GetDB();

            //�ő�̔ԍ����擾
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.�e�X�g�p�ԍ�.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            db.BeginTransaction();

            //�e�X�g�p�f�[�^�̑}��
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //�D�݂ŁE�E
            {
                //�����ʏ�̑}���@SQL���𗘗p���Ă̑}������

                //SQL�̃C���[�W
                //tbl.Field �̃L�[ insert into �e�[�u����(��)
                //tbl.Field �̒l   select ��

                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g" + System.DateTime.Now.ToString());
                //���������Ӗ�
                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g");
                //tbl[DbTable.T_TEST.TEXTDATA.Name] = db.AddInsertParameter(DbTable.T_TEST.TEXTDATA.Name, "�e�X�g");

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData);
                tbl.AddParameter(DbTable.T_TEST.�t���O.Name, true);

                tbl.AddParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one);

                //nvarchar(50)��51�������ꂽ�ꍇ�̃e�X�g�i�������DB�N���X�ł͓���Ȃ������͋�؂��ăZ�b�g�j��"��"������
                tbl.AddParameter(DbTable.T_TEST.TEXTDATA.Name, "12345678901234567890123456789012345678901234567890��");
                //���l�����I�[�o�[�����ꍇ�̃e�X�g�i�������DB�N���X�ł�null(or �f�t�H���g�l)���Z�b�g�j
                tbl.AddParameter(DbTable.T_TEST.���z.Name, double.MaxValue);

                //DbTable�ɒ�`����Ă��Ȃ��t�B�[���h��OK  (�t�B�[���h�^�C�v�́A������q�؂��邱��)
                //tbl.Field["�Ă���"] = db.AddParameter(DbTable.T_TEST.�t���O.Name, "aaaa");

                //(InsertIntoSQL���g����)�X�V������CommonData.GetDB(InsertUpdateDataParameter)���Ŏ����I�ɍX�V
                db.Execure($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        values 
                                ({tbl.InsertSelectSQL}) 
                    ");


            }

            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name, TableHelper.FIELD_GET_TYPE.Field))
            {
                Assert.True(tbl.Field.Keys.Count != 0);

                //����SQL���𗘗p���Ă̑}������
                //tbl.Field�@�ɂ� Autonumber�i�����}��off�j���������f�[�^���Z�b�g����Ă��܂��B(�擾�������ꍇ�́@TableHelper�̈����Q��)
                var oldmaxkbn = maxData;

                maxData++;
                tbl.AddParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData);
                //sql�����\
                tbl[DbTable.T_TEST.TEXTDATA.Name] = "substring(" + DbTable.T_TEST.TEXTDATA.Name + ",1,10)";     //tbl.Field[] �̏ȗ��`
                tbl[DbTable.T_TEST.���z.Name] = "1000";

                //���ݒ肳��Ă��Ȃ��f�[�^�͌��f�[�^�̒l��insert
                db.Execure($@"
                        insert into {DbTable.T_TEST.Name}
                                ({tbl.InsertIntoSQL})
                        select 
                                {tbl.InsertSelectSQL}
                        from 
                                {DbTable.T_TEST.Name}
                        where 
                                {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, oldmaxkbn)}

                    ");


                //���������悤�ȃe�[�u���ɃR�s�[�iT_TEST�̃f�[�^�ɂ�����T_TEST_COPY�ɂȂ��f�[�^���R�s�[�j����
                tbl.FieldGetType = TableHelper.FIELD_GET_TYPE.FieldAndTable;
                tbl.Reset();

                //tbl.Field[DbTable.T_TEST.TEXTDATA.Name] =  "T_TEST.TEXTDATA" �̃f�[�^�������Ă��܂��B
                //�e�[�u�������K�v�Ȏ��́@FullName�@���g�p
                tbl[DbTable.T_TEST.ID_AUTO.Name] = DbTable.T_TEST.ID_AUTO.FullName;   //T_TEST_COPY��ID_AUTO�̓I�[�g�i���o�[�ł͂Ȃ�
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

                db.Execure(sql);
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
            using (var tbl = new TableHelper(db, DbTable.T_TEST.Name))      //�D�݂ŁE�E
            {
                //����Update�e�X�g����
                tbl.AddParameter(DbTable.T_TEST.TEXTFIX.Name, "TEST");

                //����SQL�����������Ƃ��\
                tbl.Field[DbTable.T_TEST.���z.Name] = $@"
                            case when {db.AddWhereParameter(DbTable.T_TEST.���z.Name, 0, "<>")} then
                                {DbTable.T_TEST.���z.Name} + 100
                            else
                                {DbTable.T_TEST.���z.Name}
                            end
                            "
                     ;
                //(UpdateSetSQL���g����)�X�V�����Ȃǂ������ŃZ�b�g
                db.Execure($@"
                        update {DbTable.T_TEST_COPY.Name}
                        set 
                                {tbl.UpdateSetSQL}
                    ");
            }

            db.Close();

        }

        [Fact(DisplayName = "�ʏ��SELECT")]
        public void Select1()
        {

            var db = CommonData.GetDB();


            //�ő�̔ԍ����擾
            var dt = db.GetDataTable($@"
                        select 
                            isnull(max({DbTable.T_TEST.�e�X�g�p�ԍ�.Name}),0) as maxdata 
                        from
                            {DbTable.T_TEST.Name}
                ");

            int maxData = 0;
            if (dt.Rows.Count > 0)
            {
                maxData = int.Parse(dt.Rows[0]["maxdata"].ToString());
            }

            //�ʏ��where
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name} 
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData)}
                    
               ");

            Assert.Equal(dt.Rows.Count, 1);

            //����in�偡��
            var lst = new List<int>() { maxData, maxData - 1 };
            //AddWhereParameter �̏ꏊ�Ł@�e�X�g�p�ԍ� in (xx,yy) �̃f�[�^���쐬
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}  
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, lst)}
                    
               ");
            Assert.True(dt.Rows.Count > 1);


            //����enum�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, DATA.one)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //�����ȏち��
            //�e�X�g�p�ԍ�>=xx 
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�e�X�g�p�ԍ�.Name, maxData, DBMastar.WHERE_FUGO.Ijyo)}
                    
               ");

            Assert.Equal(dt.Rows.Count, 1);


            //����Like�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "1234", DBMastar.WHERE_FUGO.Like)}
               ");

            Assert.True(dt.Rows.Count > 1);

            //�����t���O�̃e�X�g����
            dt = db.GetDataTable($@"
                    select 
                        *
                    from 
                        {DbTable.T_TEST.Name}   
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.�t���O.Name, null)}
               ");

            Assert.True(dt.Rows.Count > 1);

        }


        [Fact(DisplayName = "�f�[�^�ǉ��E�X�V(Dataset)")]
        public void DBDataSet()
        {
            var db = CommonData.GetDB();

            db.BeginTransaction();

            //�����V�K�ǉ�����
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
            //100���f�[�^���쐬
            for (var i = 0; i < 100; i++)
            {
                var drNew = dt.NewRow();

                drNew[DbTable.T_TEST.TEXTMAX.Name] = nowString;
                drNew[DbTable.T_TEST.TEXTDATA.Name] = "�}��" + (i + 1).ToString();
                drNew[DbTable.T_TEST.���z.Name] = i * 1000;
                dt.Rows.Add(drNew);
            }

            //�X�V(�X�V������ CommonData.GetDB InsertUpdateDataDataRow ���ŏ���)
            db.UpdateDataSet();

            //�����X�V����
            db.OpenDataSet($@"
                    select 
                         * 
                    from 
                        {DbTable.T_TEST.Name}
                    where
                        {db.AddWhereParameter(DbTable.T_TEST.TEXTDATA.Name, "�}��", DBMastar.WHERE_FUGO.Like)}
                ", "�Ȃ܂���");


            dt = db.DataSet.Tables["�Ȃ܂���"];


            foreach (System.Data.DataRow dr in dt.Rows)
            {


                dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "�X�V";
                //dr[DbTable.T_TEST.TEXTDATA.Name] = dr[DbTable.T_TEST.TEXTDATA.Name].ToString() + "�X�V";
                dr[DbTable.T_TEST.���z.Name] = decimal.Parse(dr[DbTable.T_TEST.���z.Name].ToString()) + 100;
            }

            //�X�V
            db.UpdateDataSet("�Ȃ܂���");


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
                        ,{DbTable.T_TEST.�t���O.Name} as ��`�t�B�[���h������ɒǉ�
                    from 
                        {DbTable.T_TEST.Name}
                    where
                             {db.AddWhereParameter(DbTable.T_TEST.���z.Name, 0, ">=")}
                        and  {db.AddWhereParameter(DbTable.T_TEST.TEXTMAX.Name, nowString)}
                ";

            var dtSelect = db.GetDataTable(sql);

            //�Ō�̃f�[�^���폜
            dtSelect.Rows.Remove(dtSelect.Rows[dtSelect.Rows.Count - 1]);

            //�f�[�^��ύX
            var drSelect = dtSelect.NewRow();
            drSelect[DbTable.T_TEST.TEXTDATA.Name] = "�����Ł[��";
            drSelect[DbTable.T_TEST.���z.Name] = 10;

            if (System.IO.File.Exists(@"C:\inetpub\wwwroot\iis-85.png") == true)
            {
                FileStream fs = new FileStream(@"C:\inetpub\wwwroot\iis-85.png", FileMode.OpenOrCreate, FileAccess.Read);
                byte[] MyData = new byte[fs.Length];
                fs.Read(MyData, 0, System.Convert.ToInt32(fs.Length));

                fs.Close();

                drSelect[DbTable.T_TEST.�o�C�i���[.Name] = MyData;
            }
            drSelect[DbTable.T_TEST.�t���O.Name] = DBMastar.DBTrueValue;
            drSelect["��`�t�B�[���h������ɒǉ�"] = DBMastar.DBTrueValue;
            dtSelect.Rows.Add(drSelect);


            dtSelect.Rows[0][DbTable.T_TEST.���͓�.Name] = System.DateTime.Now.Date;
            dtSelect.Rows[0][DbTable.T_TEST.���z.Name] = 888;


            //json�ɕϊ�(web api�Ȃǂő��M)
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dtSelect);


            //json����DataTable�ɕϊ�(web api�ȂǂŎ�M)
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            //json����̖߂�(SQLServer2016 ���� json �����̂܂�table���ł���̂ł�������g�p�����ق����悢�Ǝv���@hint OPENJSON�֐� )
            var dataList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonString);
            //var dtInput = Newtonsoft.Json.JsonConvert.DeserializeObject<System.Data.DataTable>(jsonString);

            var tempTableName = "##temp" + nowString;

            if (dataList.Count > 0)
            {
                var dummyFieldChange = new Dictionary<string, string>()
                        {
                            { "��`�t�B�[���h������ɒǉ�", DbTable.T_TEST.�t���O.Name }
                        };


                var tbl = new TableHelper(db, DbTable.T_TEST.Name, paraDummyFieldChange: dummyFieldChange);
                //���݂̃f�[�^����Createtable���s��
                //ToList����ɂ́@using System.Linq;�@���K�v
                db.Execure(tbl.GetCreateSQL(tempTableName, dataList[0].Keys.ToList()));

                //�`�ʏ��
                if (true)
                {
                    //db.BeginTransaction();
                    db.ConnectionAutoClose = false; //##�e�[�u����������̂ŁE�E

                    db.OpenDataSet($"select * from  {tempTableName}");

                    var dtTemp = db.DataSet.Tables[0];
                    foreach (var data in dataList)
                    {
                        var dr = dtTemp.NewRow();
                        foreach (var key in data.Keys)
                        {
                            //��������t�B�[���h
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
                                //�o�C�i����̂ݓ��ʏ����i�I�u�W�F�N�g�����̂܂ܓ����ƃG���[�ɂȂ�̂ŁAbyte�f�[�^�Ƃ��ď����j
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


                //�`�����Łi�����Ǖ��ׂ�������܂��j
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;

                    Parallel.ForEach(dataList, data =>
                    {

                        //Parallel.ForEach ���ł�db�I�u�W�F�N�g�̎g���܂킵�͌���
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

                            dbpara.Execure(sql);
                        }
                    });
                }




                //�x��
                if (false)
                {
                    var filed = tbl.InsertIntoSQL;
                    foreach (var data in dataList)
                    {
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

                        db.Execure(sql);

                    }
                }
            }


            //�X�V
            db.BeginTransaction();
            var margeSql = SQLHelper.GetMargeSQL(db, DbTable.T_TEST.Name, $" select * from {tempTableName}");
            db.Execure(margeSql);
            try
            {
                db.Commit();
            }
            catch
            {
                db.Rollback();
            }

        }

        [Fact(DisplayName = "�p�����[�^�̕ۑ�")]
        public void SaveParameter()
        {
            var db = CommonData.GetDB();

            //�p�����[�^�̕ۑ�
            db.ClearSQLParameter();

            var lst = new List<int>() { 100, 200 };

            var sql = $@"
                 select 
                   * 
                 from
                    {DbTable.T_TEST.Name} 
                 where
                        {db.AddWhereParameter(DbTable.T_TEST.���z.Name, lst, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.���z.Name, DATA.one, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.���z.Name, System.DBNull.Value, DBMastar.WHERE_FUGO.Not)}
                    and {db.AddWhereParameter(DbTable.T_TEST.�t���O.Name, false, DBMastar.WHERE_FUGO.Not)}
            ";

            var datacount = db.GetDataTable(sql).Rows.Count;

            //json�I�u�W�F�N�g�ɕϊ�
            var save = Newtonsoft.Json.JsonConvert.SerializeObject(db.SQLParameter);
            var db2 = CommonData.GetDB();

            //�߂��i���̏ꍇSystem.DBNull.Value �� null�ɕϊ������̂�DBMastar�N���X�őΉ��ρj
            db2.SQLParameter = Newtonsoft.Json.JsonConvert.DeserializeObject<DBSQLParameter>(save);
            var dt2 = db2.GetDataTable(sql);

            Assert.Equal(datacount, dt2.Rows.Count);

        }

    }
}
