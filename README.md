# Program description #
Software to view the weather in the city. The [Avalonia](https://www.avaloniaui.net/) platform was chosen for implementation. The project based on the MVVM ([``Model-View-ViewModel``](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel)) design template.
```mermaid
flowchart LR
   View <--> ViewModel --> Model
   Model -.-> ViewModel
```

## Weather API
The [wttr.in](https://github.com/chubin/wttr.in) is used to provide weather, which is  a console-oriented weather forecast service that supports various information representation methods like terminal-oriented ANSI-sequences for console HTTP clients (curl, httpie, or wget), HTML for web browsers, or PNG for graphical viewers.

## Example of using the program

### Current weather display
![](example/example.png)

### Saving the weather for several days
![](example/saveWeather.png)
