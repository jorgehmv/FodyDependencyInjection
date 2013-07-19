Fody Dependency Injection
======================================================================

This project contains [Fody](https://github.com/Fody/Fody/) add-ins for automatic dependency injection.

There are add-ins for 

* [Ninject](https://github.com/jorgehmv/FodyDependencyInjection/wiki/Ninject)
* [Autofac](https://github.com/jorgehmv/FodyDependencyInjection/wiki/Autofac)
* [Spring.Net](https://github.com/jorgehmv/FodyDependencyInjection/wiki/Spring.Net) 

It is easy to create an add-in for any other dependency injection framework that supports configuring an existing instance (contributions welcome!).

  
### Why would I want this???
Whether an entity should be injected with Services, Validators or other kind of objects is a long discussion with 
different and valid point of views.

Being that said, if you choose to have your entities configured and do not want to do it in every constructor call, 
evenmore what if you don't have control on when an instance is created (i.e. an ORM is creating your entity) 
then this addin is for you.

