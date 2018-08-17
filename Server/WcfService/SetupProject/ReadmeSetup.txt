
This SetupProject uses Inno Setup.  
Download isetup-5.5.4-unicode.exe from http://www.jrsoftware.org/isinfo.php - NOTE the unicode.
A better studio https://www.kymoto.org/products/inno-script-studio/ 

Compile the   Main_Setup.iss  from the "Inno Setup Compiler". 

The files are in the FilesInclude.iss file, except for the sql script files that are in the MainHospLoy.iss

--- ---
Visual Studio 
I added a post build event to compile the Main_HospLoy.iss 
"C:\Program Files (x86)\Inno Setup 5\iscc.exe" “$(SolutionDir)\Main_Setup.iss”
Which creates C:\dev\LSOmni\LSOmniService\trunk\WcfService\SetupProject\Output\LSOmni.Service.Setup.exe


