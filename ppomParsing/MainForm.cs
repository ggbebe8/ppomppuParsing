using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;

namespace ppomParsing
{
    public partial class MainForm : Form
    {
        #region 전역변수

        string mURL = "";
        string mStartStr = "";
        string mLastStr = "";
        List<string> mGetInfoList = new List<string>();
        List<string> mChkInfoList = new List<string>();

        bool mExit = false;

        //메일관련 상수
        const string SMTP_SERVER = "smtp.gmail.com"; // SMTP 서버 주소
        const int SMTP_PORT = 587; // SMTP 포트
        const string MAIL_ID = "chops3812@gmail.com"; // 보내는 사람의 이메일
        const string MAIL_ID_NAME = "chops3812@gmail.com"; // 보내는사람 계정 ( 네이버 로그인 아이디 ) 
        const string MAIL_PW = "vkak2114";  // 보내는사람 패스워드 ( 네이버 로그인 패스워드 )

        #endregion 전역변수_End

        #region 생성자
        public MainForm()
        {
            InitializeComponent();
            mURL = "http://www.ppomppu.co.kr/zboard/zboard.php?id=ppomppu&page=1&divpage=53";
            mStartStr = "<font class=list_title>";
            mLastStr = "</font>";
            Log.Info("Start");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            fnFileRead();
            //fnAlarm();
        }
        #endregion 생성자_End


        #region 함수
        //파일읽고 초기화
        private void fnFileRead()
        {
            
            try
            {
                string strList = "";
                StreamReader objReader = new StreamReader(@".\List.ps");
                strList = objReader.ReadLine().Trim();
                objReader.Close();

                string[] tempItem = new string[] { };
                tempItem = strList.Split('|');

                foreach (string tmp in tempItem)
                {
                    lisbList.Items.Add(tmp);
                }
                return;
            }
            catch
            {
                return;
            }
        }

        //파일쓰기
        private void fnFileWrite()
        {
            string strSumList = "";

            if (lisbList.Items.Count > 0)
            {
                foreach (string temp in lisbList.Items)
                {
                    strSumList += temp + "|";
                }

                strSumList = strSumList.Substring(0, strSumList.Length - 1);
            }

            try
            {
                StreamWriter objWriter = new StreamWriter(@".\List.ps", false);
                objWriter.Write(strSumList);
                objWriter.Close();
                return;
            }
            catch (Exception e)
            {
                Log.Info(string.Format("fnFileWrite() : {0}", e.ToString()));
                return;
            }
        }

        //알림
        private void fnAlarm()
        {
            string strFindOK = "";

            mGetInfoList = Paser.GetInfo(mURL, mStartStr, mLastStr);
            if (mGetInfoList.Count < 1)
            {
                Log.Info(string.Format("fnAlarm() : 파싱된 정보가 없음"));
            }

            foreach (string wantToFind in lisbList.Items)
            {

                for (int i = 0; i < mGetInfoList.Count; i++)
                {
                    if (mChkInfoList.Contains(mGetInfoList[i]))
                        continue;

                    else if (mGetInfoList[i].Contains(wantToFind.ToUpper()))    //무조건 대문자로 변환
                    {
                        strFindOK += mGetInfoList[i] + Environment.NewLine;
                        mChkInfoList.Add(mGetInfoList[i]);
                    }
                }
            }

            if (mChkInfoList.Count > 100)
            {
                mChkInfoList.RemoveRange(0, 50);
            }

            if (strFindOK != "")
            {
                Telegram_Bot.Telegram_Send(strFindOK);
                //fnMailSend(strFindOK);
            }
        }

        //메일전송 
        private void fnMailSend(string p_Contents)
        {
            try
            {

                MailAddress mailFrom = new MailAddress(MAIL_ID, MAIL_ID_NAME, Encoding.UTF8); // 보내는사람의 정보를 생성
                MailAddress mailTo = new MailAddress("chops3812@gmail.com"); // 받는사람의 정보를 생성

                SmtpClient client = new SmtpClient(SMTP_SERVER, SMTP_PORT); // smtp 서버 정보를 생성

                MailMessage message = new MailMessage(mailFrom, mailTo);

                message.Subject = p_Contents.Replace("\r\n","--"); // 메일 제목 프로퍼티
                message.Body = p_Contents; // 메일의 몸체 메세지 프로퍼티
                message.BodyEncoding = Encoding.UTF8; // 메세지 인코딩 형식
                message.SubjectEncoding = Encoding.UTF8; // 제목 인코딩 형식

                client.EnableSsl = true; // SSL 사용 유무 (네이버는 SSL을 사용합니다. )
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new System.Net.NetworkCredential(MAIL_ID, MAIL_PW); // 보안인증 ( 로그인 )
                client.Send(message);  //메일 전송 

            }
            catch
            {
            }
        }

        #endregion 함수_End

        #region 이벤트

        //추가버튼
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(txtWant.Text.Trim().Replace(" ","") == "")
            {
                MessageBox.Show("빈 값 입력불가","빈 값",MessageBoxButtons.OK,MessageBoxIcon.Warning); 
                return;
            }

            lisbList.Items.Add(txtWant.Text);
            fnFileWrite();
        }

        //삭제버튼
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (txtWant.Text.Trim().Replace(" ", "") == "")
            {
                MessageBox.Show("삭제할 내용이 없음", "빈 값", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else if (!lisbList.Items.Contains(txtWant.Text))
            {
                MessageBox.Show("삭제할 이름이 옳바르지 않음", "빈 값", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                lisbList.Items.Remove(txtWant.Text);
                fnFileWrite();
            }
        }

        //내용확인버튼
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            //List<string> liInfo = Paser.GetInfo(mURL, mStartStr, mLastStr);
            List<string> liFindWhat = new List<string>();
            liFindWhat.Add(mStartStr + "|&|" + mLastStr);
            liFindWhat.Add("'eng list_vspace' colspan=2  title=" + "|&|" + ">");

            List<string> liInfo = Paser.GetHtmlInfo(mURL, liFindWhat);

            string strInfo = "";

            for (int i = 0; i < liInfo.Count; i++)
            {
                strInfo += liInfo[i] + Environment.NewLine + Environment.NewLine;
            }

            MessageBox.Show(strInfo);
        }


        //툴팁클릭
        private void ToolTipEXIT_Click(object sender, EventArgs e)
        {
            mExit = true;
            this.Close();
        }

        //노티피콘클릭--
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                return;
            }

            if (this.Visible)
                this.Hide();
            else
                this.Show();
        }

        //타이머이벤트
        private void timer1_Tick(object sender, EventArgs e)
        {
            fnAlarm();
        }

        //listbox 선택 이벤트
        private void lisbList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lisbList.SelectedIndex < 0)
                return;

            txtWant.Text = lisbList.Items[lisbList.SelectedIndex].ToString();
        }

        //폼 닫을 때, 종료이벤트
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!mExit)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        #endregion 이벤트_End



    }
}