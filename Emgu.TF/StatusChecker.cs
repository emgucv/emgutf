//----------------------------------------------------------------------------
//  Copyright (C) 2004-2020 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Emgu.TF
{
    /// <summary>
    /// A status checker, during the disposal, it will throw exception if the status is not ok.
    /// </summary>
    public class StatusChecker : Emgu.TF.Util.DisposableObject
    {
        private Status _status;
        private bool _statusNeedDispose;

        /// <summary>
        /// Create a status checker
        /// </summary>
        /// <param name="status">The status to be check, if null, one will be created</param>
        public StatusChecker(Status status)
        {
            _statusNeedDispose = false;
            _status = status;
            if (_status == null)
            {
                _status = new Status();
                _statusNeedDispose = true;
            }
        }

        /// <summary>
        /// Get the status that this status checker is monitoring
        /// </summary>
        public Status Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Release the unmanaged memory associated with this StatusChecker.
        /// </summary>
        protected override void DisposeObject()
        {
            if (_status == null)
                return;

            if (_status.Disposed)
            {
                _status = null;
                return;
            }

            if (_status.StatusCode != Status.Code.Ok)
            {
                throw new Exception(_status.Message);
            }

            if (_statusNeedDispose)
            {
                _status.Dispose();
            }
            _status = null;
        }
    }
}
