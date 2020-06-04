Master -> [![Build Status](https://dev.azure.com/anvaplus/XmlBinaryConverter/_apis/build/status/XmlBinaryConverter-master-CI?branchName=master)](https://dev.azure.com/anvaplus/XmlBinaryConverter/_build/latest?definitionId=23&branchName=master)     Develop -> [![Build Status](https://dev.azure.com/anvaplus/XmlBinaryConverter/_apis/build/status/XmlBinaryConverter-develop-CI?branchName=develop)](https://dev.azure.com/anvaplus/XmlBinaryConverter/_build/latest?definitionId=24&branchName=develop)



# XmlBinaryConverter

A simple console programm written in .NET Core that verify a XML file against XSD schema. If verification succeed, the XML file is binarized and from the XSD schema is created a header file for C/C++ embeded software. This header file is useed to access data from the binary file.



## Usage

### First Method - Q/A MODE
The console can be launched without any parameters. When launched in this mode, the user must insert all data in console, steep by steep, accordinq to the console questions (Q/A). If all data are inserted correctly, the resullt (binaty and header file) will be saved in ```C:\Users\[username]\Documents\XmlBinaryConverter```.

### Second Method - Background MODE
The programm can be launched in background using some parameters. When used in this mode, the console must be launched with the mandatory parameters. If all mandatory parameters are seated correctly, the resullt will be saved in ```C:\Users\[username]\Documents\XmlBinaryConverter``` folder, or in selected folder defined with ```-o``` parameter.

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
The console can be started in interactive mode using ```-it``` parameter. When lunched in this mode, the console will start in Background Mode by searching for all mandatory parameters, and if some of this parameters are not seated, will pass to Q/A Mode and ask the user to insert the remaining data. The resullt will be saved in ```C:\Users\[username]\Documents\XmlBinaryConverter```, or in the defined folder if ```-o``` parameter was inserted.


## License
[MIT](https://choosealicense.com/licenses/mit/)