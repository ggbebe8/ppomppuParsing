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
        #region ��������

        string mURL = "";
        string mStartStr = "";
        string mLastStr = "";
        List<string> mGetInfoList = new List<string>();
        List<string> mChkInfoList = new List<string>();

        bool mExit = false;

        //���ϰ��� ���
        const string SMTP_SERVER = "smtp.gmail.com"; // SMTP ���� �ּ�
        const int SMTP_PORT = 587; // SMTP ��Ʈ
        const string MAIL_ID = "chops3812@gmail.com"; // ������ ����� �̸���
        const string MAIL_ID_NAME = "chops3812@gmail.com"; // �����»�� ���� ( ���̹� �α��� ���̵� ) 
        const string MAIL_PW = "vkak2114";  // �����»�� �н����� ( ���̹� �α��� �н����� )

        #endregion ��������_End

        #region ������
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
        #endregion ������_End


        #region �Լ�
        //�����а� �ʱ�ȭ
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

        //���Ͼ���
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

        //�˸�
        private void fnAlarm()
        {
            string strFindOK = "";

            mGetInfoList = Paser.GetInfo(mURL, mStartStr, mLastStr);
            if (mGetInfoList.Count < 1)
            {
                Log.Info(string.Format("fnAlarm() : �Ľ̵� ������ ����"));
            }

            foreach (string wantToFind in lisbList.Items)
            {

                for (int i = 0; i < mGetInfoList.Count; i++)
                {
                    if (mChkInfoList.Contains(mGetInfoList[i]))
                        continue;

                    else if (mGetInfoList[i].Contains(wantToFind.ToUpper()))    //������ �빮�ڷ� ��ȯ
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

        //�������� 
        private void fnMailSend(string p_Contents)
        {
            try
            {

                MailAddress mailFrom = new MailAddress(MAIL_ID, MAIL_ID_NAME, Encoding.UTF8); // �����»���� ������ ����
                MailAddress mailTo = new MailAddress("chops3812@gmail.com"); // �޴»���� ������ ����

                SmtpClient client = new SmtpClient(SMTP_SERVER, SMTP_PORT); // smtp ���� ������ ����

                MailMessage message = new MailMessage(mailFrom, mailTo);

                message.Subject = p_Contents.Replace("\r\n","--"); // ���� ���� ������Ƽ
                message.Body = p_Contents; // ������ ��ü �޼��� ������Ƽ
                message.BodyEncoding = Encoding.UTF8; // �޼��� ���ڵ� ����
                message.SubjectEncoding = Encoding.UTF8; // ���� ���ڵ� ����

                client.EnableSsl = true; // SSL ��� ���� (���̹��� SSL�� ����մϴ�. )
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new System.Net.NetworkCredential(MAIL_ID, MAIL_PW); // �������� ( �α��� )
                client.Send(message);  //���� ���� 

            }
            catch
            {
            }
        }

        #endregion �Լ�_End

        #region �̺�Ʈ

        //�߰���ư
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(txtWant.Text.Trim().Replace(" ","") == "")
            {
                MessageBox.Show("�� �� �ԷºҰ�","�� ��",MessageBoxButtons.OK,MessageBoxIcon.Warning); 
                return;
            }

            lisbList.Items.Add(txtWant.Text);
            fnFileWrite();
        }

        //������ư
        private void btnDel_Click(object sender, EventArgs e)
        {
            if (txtWant.Text.Trim().Replace(" ", "") == "")
            {
                MessageBox.Show("������ ������ ����", "�� ��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            else if (!lisbList.Items.Contains(txtWant.Text))
            {
                MessageBox.Show("������ �̸��� �ǹٸ��� ����", "�� ��", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                lisbList.Items.Remove(txtWant.Text);
                fnFileWrite();
            }
        }

        //����Ȯ�ι�ư
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


        //����Ŭ��
        private void ToolTipEXIT_Click(object sender, EventArgs e)
        {
            mExit = true;
            this.Close();
        }

        //��Ƽ����Ŭ��--
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

        //Ÿ�̸��̺�Ʈ
        private void timer1_Tick(object sender, EventArgs e)
        {
            fnAlarm();
        }

        //listbox ���� �̺�Ʈ
        private void lisbList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lisbList.SelectedIndex < 0)
                return;

            txtWant.Text = lisbList.Items[lisbList.SelectedIndex].ToString();
        }

        //�� ���� ��, �����̺�Ʈ
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!mExit)
            {
                e.Cancel = true;
                this.Hide();
            }
        }
        #endregion �̺�Ʈ_End



    }
}