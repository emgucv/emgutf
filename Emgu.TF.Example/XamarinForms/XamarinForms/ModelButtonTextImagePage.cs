//----------------------------------------------------------------------------
//  Copyright (C) 2004-2019 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using System.Threading.Tasks;
#if __ANDROID__
using Android.Graphics;
#elif __IOS__
using UIKit;
using CoreGraphics;
#elif __UNIFIED__
using AppKit;
using CoreGraphics;
#endif
using Emgu.TF;
using Emgu.Models;
using Emgu.TF.Models;
using System.Net;
using System.ComponentModel;
using System.Diagnostics;

namespace Emgu.TF.XamarinForms
{
    public abstract class ModelButtonTextImagePage : ButtonTextImagePage
    {
        protected ButtonMode _buttonMode = ButtonMode.WaitingModelDownload;

        public enum ButtonMode
        {
            WaitingModelDownload,
            Ready
        }

        public ModelButtonTextImagePage():
            base()
        {
            var button = this.GetButton();
            button.Text = GetButtonName(_buttonMode);

            button.Clicked += OnButtonClicked;
        }

        public abstract String GetButtonName(ButtonMode mode);

        public virtual void OnButtonClicked(Object sender, EventArgs args)
        {
            if (_buttonMode == ButtonMode.WaitingModelDownload)
            {
                var button = this.GetButton();
                button.IsEnabled = false;
                SetMessage("Please wait while we download the model from internet.");
            }
        }

        protected void onDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SetMessage("Models downloaded.");
            var button = this.GetButton();
            _buttonMode = ButtonMode.Ready;
            button.Text = GetButtonName(_buttonMode);
            button.IsEnabled = true;

        }

        private static String ByteToSizeStr(long byteCount)
        {
            if (byteCount < 1024)
            {
                return String.Format("{0} B", byteCount);
            } else if (byteCount < 1024 * 1024)
            {
                return String.Format("{0} KB", byteCount / 1024);
            } else
            {
                return String.Format("{0} MB", byteCount / (1024 * 1024));
            }
        }

        protected void onDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            String msg;
            if (e.TotalBytesToReceive > 0)
                msg = String.Format("{0} of {1} downloaded ({2}%)", ByteToSizeStr(e.BytesReceived), ByteToSizeStr(e.TotalBytesToReceive), e.ProgressPercentage);
            else
                msg = String.Format("{0} downloaded", ByteToSizeStr(e.BytesReceived));
            SetMessage(msg);
        }
    }
}
