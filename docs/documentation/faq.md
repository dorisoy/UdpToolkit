# FAQ

**Q:** For what?  
**A:** For real-time multiplayer games. Many ideas (Sequenced Channel, RTT measurement) were taken from [GafferOnGames](https://gafferongames.com/) blog. If you start implementing these features you could see this is not simple. And for me nice to have a tool for writing multiplayer games in C#/.NET with described approaches.

**Q:** Why just not use ready project?  
**A:** Of Course already exist well-tested projects. This is my list of disadvantages of using several well-known solutions:  
- Pricing
- Poor SLA for cloud services
- Huge source code base
- Sometimes strange code base with hardcoded dependencies  
- Private sources
- Not cross platform solutions

**Q:** Ok, what about performance?  
**A:** "Premature optimization is the root of all evil." Donald Knuth (C).  
The project is still under development. And some features related to performance have not been implemented yet, because not all components have stable design and API.
I have plans for adding needed optimizations to reduce the impact of GC, this is very important for Unity3D projects and think they will appear in the project by the beta version, stay tuned.

**Q:** Why do I see other third-party libs used for serialization?  
**A:** By default `UdpToolkit` does not provide an implementation of `ISerializer`, several reasons for this:
- This is a hard task to implement your own serialization library with nice performance metrics and this is out of scope for this project.
- Not all serialization libraries support IL2CPP builds.
- Transitive dependencies are evil. In my mind, any tool should not force you to use any third-party libs, but very nice to have an expansion point, for explicit changing behavior.
- In different projects used different tools for serialization.

**Q:** Why the project uses code generation?  
**A:** Several reasons:
- Simplifying the codebase.
- IL emit or reflection API may be unsupported by runtimes on mobile devices and can cause crashes.

**Q:** Could I use only network without framework?  
**A:** Yes.