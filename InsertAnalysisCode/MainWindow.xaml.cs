using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace InsertAnalysisCode
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string NEWLINE = System.Environment.NewLine;     /// 解析文字
        private static string SMALLHEAD = "<head>" + NEWLINE;           /// Googleコード挿入先の文字列(小文字)
        private static string BIGHEAD = "<HEAD>" + NEWLINE;             /// Googleコード挿入先の文字列(大文字)

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
                checkFolderPath();

                /// HTMLにコードを挿入
                /// 
                insertCode();

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
        private void checkFolderPath()
        {
            /// フォルダが存在しない場合
            if (!Directory.Exists(this.txtFolderPath.Text))
            {
                var message = "フォルダが存在しない。";
                var error = new DirectoryNotFoundException(message);
                throw error;
            }
                
        }

        /// <summary>
        /// HTMLに解析コードを挿入する
        /// </summary>
        private void insertCode()
        {
            /// HTMLファイルのパスを取得
            /// 
            string[] filePathList = getHTMLFilePathList();

            /// <HEAD>内にコード挿入
            /// 
            insertGoogleCode(filePathList);            
        }

        /// <summary>
        /// HTML(HTM含む)ファイルパスを取得する
        /// </summary>
        /// <returns>ファイルパスリスト</returns>
        private string[] getHTMLFilePathList()
        {
            string[] htms = getFiles(@"*.htm");

            return htms;
        }

        /// <summary>
        /// フォルダ配下の指定された正規表現のファイルパスを取得する
        /// </summary>
        /// <param name="reg">正規表現</param>
        /// <returns></returns>
        private string[] getFiles(string reg)
        {
            string[] pathList = null;
            pathList = Directory.GetFiles(this.txtFolderPath.Text, reg, SearchOption.AllDirectories);

            return pathList;
        }

        /// <summary>
        /// Googleアナリティクスコードを挿入する
        /// </summary>
        /// <param name="xmlList"></param>
        private void insertGoogleCode(string[] filePathList)
        {
            /// 各xmlに対し、Codeを挿入する
            /// 
            foreach( var filePath in filePathList )
            {
                /// Googleアナリティクスコードを挿入する
                insertCodeInHead( filePath );
            }

        }

        /// <summary>
        /// HEADタグ内にGoogleアナリティクスコードを挿入する
        /// </summary>
        /// <param name="xml"></param>
        private void insertCodeInHead(string filePath)
        {
            Debug.WriteLine("[InsertCodeInHead]", filePath);

            /// ファイル内をすべて読み込む
            /// 
            var sr = new StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"));
            var s = sr.ReadToEnd();
            sr.Close();

            
            /// <head>を<head>+Googleコードに置換する
            /// <HEAD>を<HEAD>+Googleコードに置換する
            var small = s.Replace(SMALLHEAD, SMALLHEAD + this.txtBoxCode.Text + NEWLINE);
            var big = small.Replace(BIGHEAD, BIGHEAD + this.txtBoxCode.Text + NEWLINE);

            /// ファイルに書き込む
            /// 
            var sw = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_JIS"));
            sw.Write(big);
            sw.Close();
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
            /// ２つとも入力されていれば、挿入ボタンを活性化させる
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
