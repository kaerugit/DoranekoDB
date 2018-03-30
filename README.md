# リポジトリ名
 DoranekoDB

## Description
.NET Standard 2.0～  データベースアクセス用(現在SQLServerのみ)クラスです。

## Usage

### Run
サンプルの実行はSQL文を実行後

CommonData.cs　内
こちらを自分の環境に変更してください。
db.ConnectionString = @"～";        

その後
SampleAndTestプロジェクトで　メニュー　テスト-実行(or デバッグ)-全てのテスト　で実行可能です。

### Settings
Visual Studio 2017　15.3 で動作確認

## Hints
### DBTableクラスの自動作成について

## License
DoranekoDB
[LGPL3.0](LICENSE)

他プロジェクト
[MIT](LICENSE)


## Future Releases
現在、SQLServer（DBSQLServer.cs）だけの対応ですが
こちらを改造して他DB（postgresql、oracle）も作成予定
