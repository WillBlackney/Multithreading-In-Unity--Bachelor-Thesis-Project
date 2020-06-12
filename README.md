# Multithreading In Unity: Bachelor Thesis Project
### Overview
As part of the requirements of my bachelor degree in computer science, i created this project as a means of understanding how to develop multi-threaded applications within the Unity Engine. 
During the project, i investigated and experimented with a variety of different approaches to creating multi-threaded solutions, with the two key approaches being:

1.'C# Job System' approach that takes advantage of Unity's new 'Data Oriented Technology Stack' (DOTS), and
2. an 'Old School' approach which uses the tools from the .Net Framework, and System.Threading namespace.

### Project Scope
In order to help narrow the scope and focus of this project, i condensed my investigative and educational goals down into 3 questions:

1. When and why should developers use multithreading instead of coroutines?
2. How can developers use threading to improve performance and create
asynchronous functionality of operations that are restricted to the main thread?
3. How does the new 'Data Oriented Technology Stack' (DOTS) and the Unity 'C# Job
System' package change multithreading implementation?

### Unity Solution Overview
In order to adequately test, design, implement and explore the true power of all the different approaches to multi-threading in Unity, i created a simple project called 'One Thousand Zombies!'. The functionality of the application is simple: instantiate 1000 'Zombie' game objects, then move them up and down the screen. 
In order to fairly compare the performance of different multithreading approaches ('Old School' threading, Jobs System Threading, etc), each approach was implemented within in this project, and performed the exact same task of moving zombies up and down the screen.

### Thesis Documentation
You can read the full thesis documentation [here](Thesis%20Documentation/Will.Blackney.Synopsis.Final.Draft.pdf)
