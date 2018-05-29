REM go to the folder of the current script
pushd %~p0
doxygen Emgu.TF.Doxyfile
cd latex
call make
cd ..
cp latex/refman.pdf ../Emgu.TF.Unity/Assets/Emgu.TF/Documentation/Emgu.TF.Documentation.pdf
popd