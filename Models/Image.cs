﻿namespace LinkedHUCENGv2.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public Post post { get; set; }
    }
}