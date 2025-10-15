# WebScraper Solution

## Overview
The WebScraper solution is a .NET 9 project designed to scrape web data. It includes services for loading HTML, parsing documents, storing data, and managing configurations. The solution is structured into two main projects:

1. **WebScraper**: Contains the core application logic and services.
2. **WebScraper.Tests**: Contains unit tests for the application.

## Projects

### WebScraper
- **Path**: `src/WebScraper`
- **Description**: This project contains the main application logic, including services for web scraping, HTML document handling, storage, and configuration management.
- **Key Components**:
  - `Services`: Implements the core services such as `WebScraperService`, `HtmlDocumentService`, `StorageService` etc.
  - `Models`: Defines data models like `ScrapConfiguration`.

### WebScraper.Tests
- **Path**: `test/WebScraper.Tests`
- **Description**: This project contains unit tests to ensure the reliability and correctness of the WebScraper application.
- **Key Components**:
  - `Unit`: Contains unit tests for individual services.
  - `TestAssets`: Includes test data such as sample HTML files.

## Features
- **Web Scraping**: Extract data from web pages using `WebScraperService`.
- **HTML Parsing**: Parse and manipulate HTML documents with `HtmlDocumentService`.
- **Data Storage**: Store scraped data using `StorageService` and `MongoProductRepository`.
- **Configuration Management**: Manage scraping configurations with `ScrapConfigurationProvider`.
- **Unit Tests**

## Requirements
- **.NET 9 SDK**: Ensure you have the .NET 9 SDK installed.
- **Docker Desktop**: For running infrastructure (MongoDB).
