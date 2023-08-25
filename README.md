# FolderSync

## Arguments
- -s source - string value, complete path to the source folder
- -d dest - string value, complete path to the destination folder
- -i interval - an integer value, interval at which the sync will take place
- -l log - string value, complete path to the log file

All of the arguments need to be passed as command line arguments.

## Program description
&emsp;FolderSync is a program that periodically checks if two folders hierarchy is identical, in the case that it's not it creates the hierarchy and populates it with the needed files to become identical with the source folder.

&emsp;The syncronization is one-way, this means that the destination folder will be made to be identical with the source folder, not the other way around.

&emsp;The syncronization time interval can be set manually, after the time has elapsed the syncronization takes place, the program works in a loop.

&emsp;Each creation/deletion of files and folders and updates of files (in case a file has been modified in the source folder this will be overwritten in the destination) if loged in the console and in a log file at a custom path.

&emsp;The syncronization logic has been implemented manually.

## Flow Steps
0. Parsing the given arguments.
1. Getting a full list of folders and files in both the source and the destination directories.
2. Checking for missing FOLDERS in the destination directory that are present in the source directory and creating them.
3. Checking for missing FILES in the destination directory that are present in the source directory and creating them.
4. Checking for updated FILES in the source directory that need to be overwritten in the destination directory.
   - Determining which files were changed (this has been implemented to avoid overwriting files that were not changed and don't need to be synced) by calculating the MD5 checksum for both files and if they prove to be different then we overwrite.
5. Checking for files no longer present in the source directory that need to be deleted from the destination directory. Deleting files/folders will be similar with the creation of files/folders, but the other way around.
6. Now that the folders are empty of files we check for folders that need to be deleted and we delete them in the correct order, from child to parent.
7. At each step we log the needed information in the console and in the log file Log.txt at the specified path.
8. The program is made to wait for the specified interval at which point it will loop back around to the beginning (the loop starts at point 1.).
