﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CompilerCore;

namespace nwjsCompilerCLI
{
    class Program
    {
        static void Main(string[] args)
        {

            string sdkLocation;
            string projectLocation;
            string fileExtension;
            bool removeJsFiles;
            Console.WriteLine("================================================");
            Console.WriteLine("= RPG Maker MV Cook Tool (.NET Core CLI Version)");
            Console.WriteLine("= Version D1.00");
            Console.WriteLine("= Developed by AceOfAces.");
            Console.WriteLine("= Licensed under GNU General Public License v3.");
            Console.WriteLine("================================================");

            do
            {   //Ask the user where is the SDK. Check if the folder's there.
                Console.WriteLine("Where's the SDK location? ");
                sdkLocation = Console.ReadLine();
                if (sdkLocation == null) Console.WriteLine("Please insert the path for the SDK please.\n");
                else if (!Directory.Exists(sdkLocation))
                    Console.Write("The directory isn't there. Please select an existing folder.\n");
            } while (sdkLocation == null || !Directory.Exists(sdkLocation));

            do
            {  //Ask the user what project to compile. Check if the folder is there and there's a js folder.
                Console.WriteLine("\nWhere's the project you want to compile? ");
                projectLocation = Console.ReadLine();

                if (projectLocation == null) Console.WriteLine("Please specify the location of the folder.\n");
                else if (!Directory.Exists(projectLocation)) Console.WriteLine("The folder you've selected isn't present.\n");
                else if (!Directory.Exists(Path.Combine(projectLocation, "www", "js"))) Console.WriteLine("There is no js folder.\n");
            }
            while (projectLocation == null || !Directory.Exists(projectLocation) ||
                     !Directory.Exists(Path.Combine(projectLocation, "www", "js")));

            //Leave the commented code as is for now.
            //do 
            //{
            //    Console.WriteLine(
            //        "\nWould you like to:\n1. Compile JavaScript?\n2. Compile and package to package.nw?\n");
            //    modeSelected = Convert.ToInt32(Console.Read());
            //    if (modeSelected < 1 && modeSelected > 2) Console.Write("You didn't pick any of the answers. Try again.\n");
            //}
            //while (modeSelected < 1 && modeSelected > 2);

            //if (modeSelected != 3)
            //{

            //Ask the user for the file extension.
            Console.Write("\nWhat Extension will your game use (leave empty for .bin)? ");
            fileExtension = Console.ReadLine();
            if (string.IsNullOrEmpty(fileExtension)) fileExtension = "bin";
            //This is the check if the tool should delete the JS files.
            Console.WriteLine("\nDo you want to:\n1. Test that the binary files are loaded properly?\n2. Prepare for publishing?\n(Default is 1) ");
            var checkBuffer = Console.ReadLine();
            int.TryParse(checkBuffer, out var checkDeletion);
            removeJsFiles = (checkDeletion == 2);
            //}

            //The folder that the tool looks for.
            const string folderMap = "js";
            //Finding all the JS files.
            CoreCode.FileFinder(Path.Combine(projectLocation, "www",  folderMap), "*.js");
            Console.WriteLine("\n" + DateTime.Now + "\nRemoving binary files (if present)...\n");
            CoreCode.CleanupBin();
            //Preparing the compiler task.
            CoreCode.CompilerInfo.FileName = Path.Combine(sdkLocation, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "nwjc.exe" : "nwjc");
            try
            {
                //Read from the FileMap (which is located in the CompilerCore library.
                //Compilation is done in parallel. Handy for multi-core systems.
                Parallel.ForEach(CoreCode.FileMap, fileName =>
                {
                    //Print the status of the compiler. Show which thread is compiling what as well.
                    Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId + " is compiling " + fileName + "...\n");
                    //Call the compiler task.
                    CoreCode.CompilerWorkerTask(fileName, fileExtension, removeJsFiles);
                    Console.WriteLine("\n" + DateTime.Now + "\nThread #" + Thread.CurrentThread.ManagedThreadId + " finished compiling " + fileName + ".\n");
                });
                Console.WriteLine("\nFinished compiling.");

            }
            catch (Exception e)
            {
                //TODO Improve the handling of the errors.
                Console.WriteLine(e);
                throw;

            }
            //Ask the user to press Enter (or Return).
            Console.WriteLine("Push Enter/Return to exit.");
            Console.ReadLine();
        }
    }
}