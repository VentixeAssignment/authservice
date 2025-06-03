﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApi.Entities;

namespace WebApi.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<UserEntity>(options)
    {
    }
}
