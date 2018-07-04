REM go to the folder of the current script
pushd %~p0
cp NamespaceDoc.cs Assets/Emgu.TF.Lite/Assets/Scripts
doxygen Emgu.TF.Lite.Doxyfile
cd latex
call make
cd ..
cp latex/refman.pdf ../Emgu.TF.Lite.Unity/Assets/Emgu.TF.Lite/Documentation/Emgu.TF.Lite.Documentation.pdf
rm Assets/Emgu.TF.Lite/Assets/Scripts/NamespaceDoc.cs
popd