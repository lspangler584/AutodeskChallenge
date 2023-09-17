# AutodeskChallenge
Split Building Limits by Height Plateaus

### Split Building Limits API

This project provides a single API call to create a set of building limits split by height plateaus.

#### Inputs

* Building Limits - A set of coordinates representing the building limits formatted in a JSON file named SampleBuildingLimits.json. See samples dir for example.
* Height Plateaus - A set of coordinates representing height plateaus formatted in a JSON file named SampleHeightPlateaus.json. See samples dir for example.

#### Outputs

A JSON file named SplitLimits.json representing the coordinates of the split building limits by height plateaus.

#### Usage

* Input files are located in .../SplitBuildingLimits/samples.
* Open solution in Visual Studio 2022.
* Run using Debug -> Start Debugging or Debug ->Start Without Debugging.

SampleLimits.json output file is written to .../SplitBuildingLimits/samples.

Future - Build as an executable, parameterize input files and location of output file.

#### Testing (Future)

* Add test project to solution
* Use test infrastructure to automate scenarios:
  * Invalid inputs
  * Polygons with large number of coordinates
  * Inputs with large number of Polygons
  * Mechanism to validate output

#### Deployment (Future)
* Publish the .dll to AWS S3 and set access permissions for allowed uses/users.
* Publish the .dll to Azure Blob Storage, as an Azure Function, or Azure App Service.
* Google Cloud, etc.

#### Credits

Autodesk and me.

