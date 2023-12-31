﻿using AutoMapper;
using EntityFramework.Exceptions.Common;
using web2server.Dtos;
using web2server.Exceptions;
using web2server.Infrastructure;
using web2server.Interfaces;
using web2server.Models;
using web2server.QueryParametars;

namespace web2server.Services
{
    public class ArticleService : IArticleService
    {
        private readonly WebshopDbContext _dbContext;
        private readonly IMapper _mapper;

        public ArticleService(WebshopDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public ArticleResponseDto CreateArticle(ArticleRequestDto requestDto, long userId)
        {
            Article article = _mapper.Map<Article>(requestDto);
            article.SellerId = userId;

            _dbContext.Articles.Add(article);

            try
            {
                _dbContext.SaveChanges();
            }
            catch (CannotInsertNullException)
            {
                throw new InvalidFieldsException("One of more fields are missing!");
            }
            catch (Exception)
            {
                throw;
            }

            return _mapper.Map<ArticleResponseDto>(article);
        }

        public DeleteResponseDto DeleteArticle(long id, long userId)
        {
            Article article = _dbContext.Articles.Find(id);

            if (article == null)
            {
                throw new ResourceNotFoundException("Article with specified id doesn't exist!");
            }

            if (article.SellerId != userId)
            {
                throw new ForbiddenActionException("Sellers can only delete their own articles!");
            }

            _dbContext.Articles.Remove(article);
            _dbContext.SaveChanges();

            return _mapper.Map<DeleteResponseDto>(article);
        }

        public List<ArticleResponseDto> GetAllArticles(ArticleQueryParameters queryParameters)
        {
            List<Article> articles = new List<Article>();

            if (queryParameters.SellerId > 0)
            {
                articles = _dbContext.Articles.Where(x => x.SellerId == queryParameters.SellerId).ToList();
            }
            else
            {
                articles = _dbContext.Articles.ToList();
            }

            return _mapper.Map<List<ArticleResponseDto>>(articles);
        }

        public ArticleResponseDto GetArticleById(long id)
        {
            ArticleResponseDto article = _mapper.Map<ArticleResponseDto>(_dbContext.Articles.Find(id));

            if (article == null)
            {
                throw new ResourceNotFoundException("Article with specified id doesn't exist!");
            }

            return article;
        }

        public ArticleResponseDto UpdateArticle(long id, ArticleRequestDto requestDto, long userId)
        {
            Article article = _dbContext.Articles.Find(id);

            if (article == null)
            {
                throw new ResourceNotFoundException("Article with specified id doesn't exist!");
            }

            if (article.SellerId != userId)
            {
                throw new ForbiddenActionException("Sellers can only modify their own articles!");
            }

            _mapper.Map(requestDto, article);

            try
            {
                _dbContext.SaveChanges();
            }
            catch (CannotInsertNullException)
            {
                throw new InvalidFieldsException("One of more fields are missing!");
            }
            catch (Exception)
            {
                throw;
            }

            return _mapper.Map<ArticleResponseDto>(article);
        }
    }
}
