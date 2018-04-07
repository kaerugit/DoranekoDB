# DoranekoDB
.NET Standard 2.0～  データベースアクセス用(現在SQLServerのみ)クラスです。  
簡易（疑似）ormというか、DBの記述を効率化出来ます。  

## Description
entity framework で問題なければそちらの利用をお勧めします。  
SQL文をゴリゴリ書いて、DataTable（DataSet）が沢山出てくるようなプロジェクトにお勧めします。  
⇒ORMというより、パラメータ化の簡易化及びインテリジェンスによるテーブルフィールドの恩恵を最大限に利用できるクラスです。  
　⇒DBの構造が変更した場合は、コンパイルエラーとなりますので、DBの変更に適時対応できる形式となっております。  

## Usage
### Sample
パラメータ込みのSQL文を簡易的に発行します。  
⇒DbTable.〇〇は　現在のテーブル定義より自動作成（後述）します。  
　⇒(VSのバージョンにもよりますが)見た目SQL文のような記述が可能となります。  

◇通常のSQL

dt = db.GetDataTable($@"  
        select   
            *  
        from   
            {DbTable.T_TEST.Name}   
        where  
            {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, 1)}　・・・①               
    ");

※①の場所がパラメータ化(テスト用番号=@1)されます。  
⇒ >= <= などの記述も可能です。  

◇通常のSQL(IN句)　　AddWhereParameter の場所で　テスト用番号 in (xx,yy) のデータが作成

var lst = new List<int>() { 1, 2 };  
dt = db.GetDataTable($@"  
        select   
            *  
        from   
            {DbTable.T_TEST.Name}    
        where  
            {db.AddWhereParameter(DbTable.T_TEST.テスト用番号.Name, lst)}           
    ");

### DemoRun
SQLServerにDB（任意）を作成  
[SQL](SQL.txt)　を実行  

CommonData.cs　内  
こちらを各自の環境に変更してください。  
db.ConnectionString = @"～";  

その後  
SampleAndTestプロジェクトで　メニュー　テスト-実行(or デバッグ)-全てのテスト　で実行可能です。

※SampleAndTestプロジェクト内に色々なサンプルがあります。

### Settings
Visual Studio 2017　15.3  
SQLServer2017  
上記で、動作確認  
※ほかのコンポーネントはNugetで取得お願いします。  

### Project
デモを見て、本気で使ってみたい方は  
DoranekoDBプロジェクトとProjectCommonプロジェクト（要改造）が必要となります。

以下、各プロジェクトの大まかな説明です。  
DoranekoDBプロジェクト　　　→　データベースアクセス(SQL文発行)本体  
ProjectCommonプロジェクト　 →　各自業務にあった共通のプロジェクト  
SampleAndTestプロジェクト　 →　テスト用プロジェクト  
TemplateHelperプロジェクト　→　DBTable作成用プロジェクト

## Hints
### DBTableクラスの自動作成について
TemplateHelperプロジェクトをコンパイル後  
TemplateHelper.dll　が作成されたフォルダで  
以下のコマンドを実行  

C#  
dotnet TemplateHelper.dll --connectionstring "（接続文字列）" --lang CS

VB.Net  
dotnet TemplateHelper.dll --connectionstring "（接続文字列）" --lang VB

実行後  
CommonDataInfo.cs(vb)が作成されるので  
こちらのファイルを移動して利用してください。

## License
DoranekoDBプロジェクト  
[LGPL3.0](LICENSE.txt)

他プロジェクト  
[MIT](LICENSEMIT.txt)

## Future Releases
現在、SQLServer（DBSQLServer.cs）だけの対応ですが  
こちらを改造して他DB（postgresql、oracle）も作成予定
