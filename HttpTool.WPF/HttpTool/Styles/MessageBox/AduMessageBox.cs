using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HttpTool.Styles.MessageBox
{
    //类似工厂模式显示不同风格的MessageBox
    public class AduMessageBox
    {
        public static int OK = 0;
        public static int OKCANCLE = 1;
        public AduMessageBox() { }

        public static void Show(string mess, BitmapSource BackImgSour = null)
        {
            MessageBoxOK AduMessageBoxOK = new MessageBoxOK(mess);
            if (BackImgSour != null)
            {
                ImageBrush img = new ImageBrush(BackImgSour);
                img.Stretch = Stretch.UniformToFill;
                AduMessageBoxOK = new MessageBoxOK(mess, BackImgSour);
            }
            AduMessageBoxOK.Show();

        }
        public static void ShowAutoClose(string mess, BitmapSource BackImgSour = null)
        {
            MessageBox_AutoShutdown AduMessageBoxOK = new MessageBox_AutoShutdown(mess);
            if (BackImgSour != null)
            {
                ImageBrush img = new ImageBrush(BackImgSour);
                img.Stretch = Stretch.UniformToFill;
                //AduMessageBoxOK = new MessageBox_AutoShutdown(mess, BackImgSour);
            }
            AduMessageBoxOK.Show();

        }
        /// <summary>
        /// 显示对话框
        /// </summary>
        /// <param name="mess">提示消息</param>
        /// <param name="style">对话框样式</param>
        public static bool? ShowDialog(string mess, int style, BitmapSource backImgSour=null)
        {
            switch (style)
            {
                case 0:
                    MessageBoxOK AduMessageBoxOK = new MessageBoxOK(mess,backImgSour);
                    return AduMessageBoxOK.ShowDialog();
                case 1:
                    MessageBoxOKCancle AduMessageBoxOKCancle = new MessageBoxOKCancle(mess,backImgSour);
                    return AduMessageBoxOKCancle.ShowDialog();
                default:
                    return false;
            }

        }

    }
}
