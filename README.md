# Instagram Clone – Backend API

> *An ASP.NET Core backend that powers an Instagram-style social media app: posts, stories, likes, follows, DMs, and notifications.*

---

## Table of Contents

1. [Overview](#overview)
2. [Features](#features)
3. [Tech Stack](#tech-stack)
4. [Architecture](#architecture)
5. [API Reference](#api-reference)

---

## Overview

This project is the **backend service** for an Instagram-like application. It exposes a RESTful API that lets clients:

* register & authenticate users,
* publish posts and stories with media uploads,
* like, comment, and follow other users,
* exchange direct messages,
* receive real-time notifications.

Everything is built with **.NET 9** and follows a clean, layered architecture so it can grow pain-free.

---

## Features

* **Layered solution structure** – `API → Business → Data → Entities`.
* **Identity**-based authentication with JWT & refresh tokens.
* **Follow, like, comment, story, message** domains implemented.
* **Soft-delete & audit fields** on all entities.
* **OpenAPI / Swagger** UI generated automatically.
* **Mermaid ER diagram** committed for instant DB insight.


---

## Tech Stack

| Layer           | Library / Tool                         | Purpose                              |
|-----------------|----------------------------------------|--------------------------------------|
| **API**         | ASP.NET Core 9 Web API                 | Controllers & middleware (authN/Z)   |
| **Business**    | AutoMapper                             | Application services, DTO↔entity map |
| **Data Access** | Entity Framework Core 9 · Tools/Design | Repositories, migrations             |
| **Entities**    | C# Class Library                       | Domain models (AppUser, Post, …)     |
| **Database**    | PostgreSQL 17                          | Persistent storage                   |


---

## Architecture

### Layer Diagram

```mermaid
graph LR
  UI[Mobile / Web Client] -->|HTTP| API[API Layer]
  API -->|Service Call| BL[Business Layer]
  BL -->|Repository| DAL[Data Access Layer]
  DAL -->|EF Core| DB[(PostgreSQL 17)]
  ENT[Entities]
  BL -.-> ENT
  DAL -.-> ENT
```

### Entity-Relationship Diagram

```mermaid
%%{ init: { "er": { "attributeStyle": "label" } } }%%
erDiagram
    AppUser {
        GUID     Id PK
        string   FullName
        string  ProfilePictureUrl
        string  Bio
        string  WebsiteUrl
        datetime CreatedAt
        datetime UpdatedAt
        datetime LastLoginDate
        boolean  IsActive
        boolean  IsDeleted
        boolean  IsPrivate
    }

    Post {
        int      Id PK
        GUID     AuthorId FK
        string   ImageUrl
        string  Caption
        datetime CreatedAt
        datetime ModifiedAt
        boolean  IsDeleted
        datetime DeletedAt
    }

    Comment {
        int      Id PK
        int      PostId FK
        GUID     AuthorId FK
        int     ParentCommentId FK
        string   Content
        datetime CreatedAt
        boolean  IsDeleted
    }

    PostLike {
        GUID     UserId PK, FK
        int      PostId PK, FK
        datetime CreatedAt
        boolean  IsDeleted
    }

    CommentLike {
        GUID     UserId PK, FK
        int      CommentId PK, FK
        datetime CreatedAt
        boolean  IsDeleted
    }

    Follow {
        GUID     FollowerId PK, FK
        GUID     FollowedId PK, FK
        datetime CreatedAt
        datetime DecidedAt
        boolean  IsDeleted
        FollowStatus Status
    }

    Message {
        int      Id PK
        GUID     SenderId FK
        GUID     ReceiverId FK
        string   Content
        datetime CreatedAt
        boolean  IsRead
        boolean  IsDeleted
    }

    Notification {
        int      Id PK
        GUID     RecipientId FK
        NotificationType Type
        string   Message
        string  ActionUrl
        boolean  IsRead
        datetime CreatedAt
        boolean  IsDeleted
        GUID    ActorId FK
        int     PostId FK
        int     CommentId FK
    }

    Story {
        int      Id PK
        GUID     UserId FK
        string   MediaUrl
        datetime CreatedAt
        datetime ExpiresAt
    }

    StoryView {
        GUID     UserId PK, FK
        int      StoryId PK, FK
        datetime ViewedAt
    }

    %% Relationships
    AppUser ||--o{ Post        : authored
    AppUser ||--o{ Comment     : writes
    AppUser ||--o{ PostLike    : likes
    AppUser ||--o{ CommentLike : likes
    AppUser ||--o{ Follow      : follows
    Follow  }o--|| AppUser     : followed
    AppUser ||--o{ Message     : sends
    AppUser ||--o{ Message     : receives
    AppUser ||--o{ Notification: notified
    AppUser ||--o{ Story       : posts
    AppUser ||--o{ StoryView   : viewer

    Post    ||--o{ Comment     : has
    Post    ||--o{ PostLike    : liked
    Post    ||--o{ Notification: involved

    Comment ||--o{ Comment     : replies
    Comment ||--o{ CommentLike : liked
    Comment ||--o{ Notification: involved

    Story   ||--o{ StoryView   : viewed
```

---


## API Reference

| Method | Endpoint                  | Description                    |
| ------ | ------------------------- | ------------------------------ |
| POST   | `/api/v1/auth/register`   | Register a new account         |
| POST   | `/api/v1/auth/login`      | Obtain JWT access & refresh    |
| GET    | `/api/v1/users`           | List users                     |
| GET    | `/api/v1/users/{id}`      | Retrieve user profile          |
| POST   | `/api/v1/posts`           | Create a post                  |
| GET    | `/api/v1/posts/{id}`      | Get a single post              |
| POST   | `/api/v1/posts/{id}/like` | Like a post                    |
| ...    | ...                       | *(see Swagger for all routes)* |

Full, always-up-to-date docs live at `/swagger`.
