# RebusHandlerException

## Purpose
The purpose of this application is to demonstate what I believe is an issue with Rebus. This occurs when using a RabbitMQ transport and multiple instances of an application are running against the same input queue. 

This scenario applies only when an exception is thrown in a message handler. The message is immediately handled by the other running application, even if the bus is not configured for any message retries. 

For the same scenario, but when an exception does not occur in the messagehandler, the message is successfully being handled just a single time. This across all running applications.

## Prerequisites
A RabbitMQ environment must exist locally, running on localhost with the default ports of 15672:5672. There must be a virtual host of 'test' with access enabled for a user with name 'admin' and a password of 'password'. These variables can easily be changed in the source code within the Program.cs files near the top.

## How to Run
The code to build the executables is here in this repository within the base directory. 

- Rebus.Handler1.exe (Single file app of Rebus.Handler)
- Rebus.Handler2.exe (Single file app of Rebus.Handler - copy)
- Rebus.HandlerWithException1.exe (Single file app of Rebus.HandlerWithException)
- Rebus.HandlerWithException2.exe (Single file app of Rebus.HandlerWithException - copy)
- Rebus.Publisher.exe (Single file app of Rebus.Publisher)

These applications were all built by opening the terminal in Visual Studio with the solution loaded and using the following command:

```
dotnet publish -r win-x64
```

Then the generated executables from the bin\debug\.net6.0\win-x64\publish directory within each project can be copied to overwrite the existing executables in the main solution directory.

Running all 5 applications at the same time will generate 2 consumer queues in RabbitMQ: RebusHandler and RebusHandlerException

The publisher application will prompt you to press any key to publish a message. This message will end up in each of the above RabbitMQ queues.

## Current Result

- The Rebus.Handler applications do not throw an exception and work properly. The message is consumed by one of the 2 running applications.
- The Rebus.HandlerWithExceptions applications throw an exception (on purpose) and demonstrate that this exception throwing causes the message to be consumed by each application, despite any retries being disabled.

## Desired Result

- The Rebus.Handler and Rebus.HandlerWithExceptions applications only handle the published message a single time.
