//----------------------------------------------------------------------------
//  Copyright (C) 2004-2025 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Structure;

#if VS_TEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
#elif NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestAttribute = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Trace = System.Diagnostics.Debug;
using System.Threading.Tasks;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
#else
using NUnit.Framework;
#endif

namespace Emgu.TF.Test
{
    public static class EmguAssert
    {

#if __IOS__ || __ANDROID__
        public static void IsTrue(bool condition)
        {
            Assert.That(condition);
        }

        public static void IsTrue(bool condition, String message)
        {
            Assert.That(condition, message);
        }

        public static void AreEqual(object a, object b)
        {
            Assert.That(a.Equals(b));
        }

        public static void AreEqual(object a, object b, string message)
        {
            Assert.That(a.Equals(b), message);
        }

        public static void AreNotEqual(object a, object b, string message)
        {
            Assert.That(a.Equals(b), Is.False, message);
        }

        public static void IsFalse(bool condition)
        {
            Assert.That(condition, Is.False);
        }

        public static void WriteLine(String message)
        {
            Console.WriteLine(message);
        }
#else
        public static void IsTrue(bool condition)
        {
#if VS_TEST || NETFX_CORE
            Assert.IsTrue(condition);
#else
            Assert.That(condition);
#endif
        }

        public static void IsTrue(bool condition, String message)
        {
#if VS_TEST || NETFX_CORE
            Assert.IsTrue(condition, message);
#else
            Assert.That(condition, message);
#endif
        }

        public static void AreEqual(object a, object b)
        {
#if VS_TEST || NETFX_CORE
            Assert.AreEqual(a, b);
#else
            Assert.That(a.Equals(b));
#endif
        }

        public static void AreEqual(object a, object b, string message)
        {
#if VS_TEST || NETFX_CORE
            Assert.AreEqual(a, b, message);
#else
            Assert.That(a.Equals(b), message);
#endif
        }

        public static void AreNotEqual(object a, object b, string message)
        {
#if VS_TEST || NETFX_CORE
            Assert.AreNotEqual(a, b, message);
#else
            Assert.That(a.Equals(b), Is.False, message);
#endif
        }

        public static void IsFalse(bool condition)
        {
#if VS_TEST || NETFX_CORE
            Assert.IsFalse(condition);
#else
            Assert.That(condition, Is.False);
#endif
        }

        public static void WriteLine(String message)
        {
            Trace.WriteLine(message);
        }

#endif
        }
}