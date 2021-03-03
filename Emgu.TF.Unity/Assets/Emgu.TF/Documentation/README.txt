EMGU TF
--------------------------------------------
Emgu TF is a .NET wrapper to the Google Tensorflow library. Allowing Tensorflow functions to be called from Unity. 

Demo scene can be found under the Demo folder.

For information & tutorials please visit http://www.emgu.com/wiki/index.php/Emgu_TF

For questions please contact us by email at support@emgu.com



About License
--------------------------------------------
Emgu TF Unity use Unity Store license for our C# wrapper implementation. However, Emgu TF also use Tensorflow, which use their own licenses. You can find the license terms in tensorflow.license.txt. Please review the license terms and make sure you compiles with them.



Important Note
--------------------------------------------
Our demos on Mac OS require reference to "System.IO.Compression.FileSystem.dll". In order to run the demos on Mac OS, please copy the "csc.rsp" file under the "Assets" folder to your root "Assets" folder (Note: not the "Assets" folder under "Emgu.TF" asset, but to the "Assets" folder of your project) in order to include the reference. For details, please visit this page:
https://forum.unity.com/threads/c-compression-zip-missing.577492/

If you do not need to run our demo, you can remove the "demo" folder from Emgu.TF asset, this way you will no longer required to reference "System.IO.Compression.FileSystem.dll"