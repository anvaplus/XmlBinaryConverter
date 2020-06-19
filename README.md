Master -> [![Build Status](https://dev.azure.com/anvaplus/XmlBinaryConverter/_apis/build/status/XmlBinaryConverter-master-CI?branchName=master)](https://dev.azure.com/anvaplus/XmlBinaryConverter/_build/latest?definitionId=23&branchName=master)     Develop -> [![Build Status](https://dev.azure.com/anvaplus/XmlBinaryConverter/_apis/build/status/XmlBinaryConverter-develop-CI?branchName=develop)](https://dev.azure.com/anvaplus/XmlBinaryConverter/_build/latest?definitionId=24&branchName=develop)



# XmlBinaryConverter

A simple console program written in .NET Core that verifies an XML file against XSD schema. the XML file is binarized and a header file for C/C++ embedded software is created from the XSD schema. This header file is useed to access data from the binary file.



## Usage

### First Method - Q/A MODE
The console can be launched without any parameters. When launched in this mode, the user must insert all data in the console, step by step, accordinq to the console questions (Q/A). If all data is inserted correctly, the result (binary and header file) will be saved in ```C:\Users\[username]\Documents\XmlBinaryConverter```.

### Second Method - Background MODE
The program can be launched in background using some parameters. When used in this mode, the console must be launched with the mandatory parameters. If all mandatory parameters are seated correctly, the result will be saved in the ```C:\Users\[username]\Documents\XmlBinaryConverter``` folder, or in the selected folder defined with ```-o``` parameter.

```
-help       Show help option            Optional
-xsd        Location of XSD file        Mandatory
-xml        Location of XML file        Mandatory
-xns        Namespace URI               Mandatory
-xpx        XML namespace prefix        Mandatory
-xen        XML main element name       Mandatory
-o          Location of output files    Optional
-it         Interactive Mode            Optional
```

### Third Method - Interactive MODE
The console can be started in interactive mode using ```-it``` parameter. When launched in this mode, the console will start in Background Mode by searching for all mandatory parameters; if any of these parameters are not set, the console will switch to Q/A Mode and ask the user to insert the missing data. The result will be saved in ```C:\Users\[username]\Documents\XmlBinaryConverter```, or in the defined folder if ```-o``` parameter was inserted.


## License
[MIT](https://choosealicense.com/licenses/mit/)