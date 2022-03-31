
install package Microsoft.EntityFrameworkCore.SqlServer

install Microsoft.EntityFrameworkCore.Design & Microsoft.EntityFrameworkCore.Tools to the main entry project and set it as a stratup project
open package manager select project Products.Data,where your migration files will go

Get-Help entityframeworkcore

Add-Migration InitialMigration
Update-Database
Add-Migration nextmigration
Remove-Migration

