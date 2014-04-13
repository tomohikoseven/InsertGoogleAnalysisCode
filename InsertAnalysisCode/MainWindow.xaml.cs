using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Sgml;

namespace InsertAnalysisCode
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";

            //ルートフォルダを指定する
            //デフォルトでDesktop
            fbd.RootFolder = Environment.SpecialFolder.Desktop;

            //最初に選択するフォルダを指定する
            //RootFolder以下にあるフォルダである必要がある
            fbd.SelectedPath = @"C:\Windows";

            //ユーザーが新しいフォルダを作成できるようにする
            //デフォルトでTrue
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog().ToString().Equals("OK"))
            {
                //選択されたフォルダを表示する
                this.txtFolderPath.Text = fbd.SelectedPath;
            }

            fbd.Dispose();

        }

        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /// フォルダパスチェック
                /// 
                CheckFolderPath();

                /// HTMLにコードを挿入
                /// 
                InsertCode();

                /// 処理が終了したメッセージ表示
                /// 
                System.Windows.Forms.MessageBox.Show("処理が終了しました。","終了");
            }
            /// すべてここでエラーを表示する。
            catch (DirectoryNotFoundException dnfe)
            {
                System.Windows.Forms.MessageBox.Show(dnfe.Message, "エラー");
            }
            catch (NotImplementedException nie)
            {
                System.Windows.Forms.MessageBox.Show(nie.Message, "エラー");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("管理者へ連絡したほうがよい。\r\n" + ex.StackTrace, "予期しないエラー");
            }
            
        }

        /// <summary>
        /// フォルダの存在チェック
        /// </summary>
        private void CheckFolderPath()
        {
            /// フォルダが存在しない場合
            if (!Directory.Exists(this.txtFolderPath.Text))
            {
                var message = "フォルダが存在しない。";
                var error = new DirectoryNotFoundException(message);
                throw error;
            }
        }

        private void InsertCode()
        {
            /// HTML Get
            /// 
            string[] filePathList = GetHTMLFilePathList();

            /// XMLに変換
            /// 
            //List<Document> xmlList = CreateXml(filePathList);

            //Debug.WriteLine("CreateXml End.");

            /// <HEAD>内にコード挿入
            /// 
            InsertGoogleCode(filePathList);

            /// ファイルへ上書きする
            /// 
            //ReWriteFile(xmlList);
            
        }

        //private void ReWriteFile(List<Document> docList)
        //{
        //    foreach (var doc in docList)
        //    {
        //        /// ファイル出力する
        //        /// 
        //        doc.XML.Save(doc.Path);
        //    }
        //}

        /// <summary>
        /// Googleアナリティクスコードを挿入する
        /// </summary>
        /// <param name="xmlList"></param>
        private void InsertGoogleCode(string[] filePathList)
        {
            /// 各xmlに対し、Codeを挿入する
            /// 
            foreach( var filePath in filePathList )
            {
                /// Googleアナリティクスコードを挿入する
                InsertCodeInHead( filePath );
            }

        }

        private static string NEWLINE = System.Environment.NewLine;
        private static string HEAD = "<head>" + NEWLINE;

        /// <summary>
        /// HEADタグ内にGoogleアナリティクスコードを挿入する
        /// </summary>
        /// <param name="xml"></param>
        private void InsertCodeInHead(string filePath)
        {
            Debug.WriteLine("[InsertCodeInHead]", filePath);

            /// ファイル内をすべて読み込む
            /// 
            var sr = new StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"));
            var s = sr.ReadToEnd();
            sr.Close();

            
            /// <head>を<head>+Googleコードに置換する
            /// 
            var rs = s.Replace(HEAD, HEAD + this.txtBoxCode.Text + NEWLINE);

            /// ファイルに書き込む
            /// 
            var sw = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw.Write(rs);
            sw.Close();
             
            /// HEADタグを抽出する
            /// 
            //XElement head = xml.XML.Element("HEAD");

            ///// HEADタグ内にGoogleCodeを追加する
            ///// 
            //head.Add( this.txtBoxCode );
        }

        /// <summary>
        /// HTMLをXMLへ変換する
        /// </summary>
        /// <param name="filePathList"></param>
        /// <returns></returns>
        //private List<Document> CreateXml(string[] filePathList)
        //{

        //    var xmlList = new List<Document>();

        //    /// XMLへ変換する
        //    foreach (var filePath in filePathList)
        //    {
        //        /// ファイル開く
        //        var xml = XDocument.Load( filePath );

        //        var doc = new Document(filePath);
        //        doc.XML = xml;
                
        //        xmlList.Add(doc);
        //    }

        //    return xmlList;
        //}

        private XDocument XmlFmHtml(string filePath)
        {
            XDocument xml;
            using (var sgmlReader = new SgmlReader() { Href = filePath })
            {
                xml = XDocument.Load(sgmlReader);
            }

            return xml;
        }

        private string[] GetHTMLFilePathList()
        {
            string[] htmls = GetFiles( "*.html" );
            string[] htms = GetFiles("*.htm");

            return htmls.Concat( htms ).ToArray();
        }

        /// <summary>
        /// フォルダ配下の指定された正規表現のファイルパスを取得する
        /// </summary>
        /// <param name="reg">正規表現</param>
        /// <returns></returns>
        private string[] GetFiles(string reg)
        {
            string[] pathList;

            pathList = Directory.GetFiles( this.txtFolderPath.Text, reg, SearchOption.AllDirectories );

            return pathList; 
        }


        private void txtFolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            /// 挿入ボタンの活性非活性制御
            /// 
            btnInsertVisible();
        }

        /// <summary>
        /// 挿入ボタンの活性非活性制御
        /// </summary>
        private void btnInsertVisible()
        {
            /// フォルダテキストボックスとGoogleコード内の判定
            /// 
            if (txtFolderPath.Text == string.Empty ||
                    txtBoxCode.Text == string.Empty)
            {
                /// 挿入ボタンの非活性
                btnInsert.IsEnabled = false;
            }
            else
            {
                /// 挿入ボタンの活性
                btnInsert.IsEnabled = true;
            }
        }

        private void txtBoxCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            /// 挿入ボタンの活性非活性制御
            /// 
            btnInsertVisible();
        }
    }
}
