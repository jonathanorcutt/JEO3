# JEO3

> **Entity Framework before the Framework took over.**

A high-throughput, provider-agnostic database framework that reflects live relational database metadata directly via raw ADO.NET—constructing a strongly-typed database object model without Entity Framework or Dapper dependencies.

[🌐 Live Showcase](https://jeo3.com) | [💻 GitHub Repo](https://github.com/jonathanorcutt/JEO3)

---

## Overview

With just a connection string and provider name, **JEO3** inspects system metadata in parallel to assemble a full, consumable database context in memory. 

It generates strongly-typed POCOs, relationship graphs, and lightweight context abstractions ready for instant integration into existing .NET codebases. The embeddable provider libraries supply zero-overhead, provider-agnostic CRUD foundations—giving you absolute control over data access without framework bloat.

## Under the Hood

* **Parallel Catalog Extraction:** Concurrent catalog metadata ingestion tested and proven against enterprise databases exceeding 300+ tables.
* **$O(1)$ Graph Topology Stitching:** Front-loaded lookup structures using zero-allocation `ValueTuple` struct keys to eliminate heap thrashing during table/index/FK graph assembly.
* **Zero ORM Tax:** Bypasses reflection inflation, deferred execution traps, and abstraction overhead by operating directly on native ADO.NET data streams.
* **Modern .NET 10 Showcase:** Built with ASP.NET Core, Blazor, and Radzen 11.5.x featuring 3D WebGL schema visualization and real-time SQL Server DMV telemetry monitoring.

---

<img width="1919" alt="JEO3 Showcase Platform" src="https://github.com/user-attachments/assets/480b434d-2883-457b-bc01-4d84ad7d10d0" />

<img width="1911" height="914" alt="Screenshot 2026-10-07 025355 a" src="https://github.com/user-attachments/assets/799d9deb-82e7-4e0a-b7af-d8011ac8fbbd" />

<img width="1919" height="917" alt="image" src="https://github.com/user-attachments/assets/a5cb463a-93d6-4f31-8a8c-a8330da14bee" />

<img width="1919" height="1079" alt="Screenshot 2026-10-07 031134 f" src="https://github.com/user-attachments/assets/e2f153b7-7f58-4dd0-8145-733e529f4ae9" />

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/2fa28516-3555-4dd0-9541-621a10079c92" />

<img width="1919" height="1079" alt="Screenshot 2026-09-20 064144" src="https://github.com/user-attachments/assets/00a2121c-d7ef-423a-a047-f3c375b7fd57" />

<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/83233690-81ff-4e1a-a2f3-49fced4801c0" />

<img width="1624" height="2378" alt="image" src="https://github.com/user-attachments/assets/44af658f-e593-45b7-8050-4fa34a689a6a" />

<img width="1919" height="913" alt="Screenshot 2026-09-20 060758" src="https://github.com/user-attachments/assets/394dface-3768-4e46-86be-dae3d954dfab" />

<img width="1919" height="1079" alt="Screenshot 2026-10-07 030701 d" src="https://github.com/user-attachments/assets/e566bfae-32df-4dc0-9698-62bf31d54a99" />



