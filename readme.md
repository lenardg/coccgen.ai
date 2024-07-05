# Call of Cthulhu Character Generator with AI support

This is a small proof of concept project that puts together several experiments I did using OpenAI GPT 
and Call of Cthulhu RPG character generation. 

The character generation inside uses the simple rules of assignment, instead of rolling and calculating 
everything. That is also doable, but here the plan was to test the workings of AI and calculation 
together to make a full character.

## Requirements

This was designed using .NET 8, to be run under the console.

Althouth it should be able to run under Linux, MacOS or Windows, currently at least Linux seems to
be broken. This might be because of the OpenAI library. 

You will need an OpenAI API key. Create a new file called `apikey.txt`, and this should contain 
only your API key. Put it in the root of the CoCCgen folder. The app will complain if the file 
is missing. 

## Usage

Currently everything is pretty much hardcoded.

In Program.cs you can setup some parameters:

```
    var country = "Great Britain";
    var lang = "English";
    var time = "present time";
```

These affect the environment of the character. 

```
    var shortRequest = Characters.Ideas.Choose();
```

This selects one of the premade character ideas. You can just fill shortRequest with something
you like or have the app read from the console.

All characters are saved as TXT file. 