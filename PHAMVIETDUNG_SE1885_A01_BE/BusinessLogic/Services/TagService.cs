using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Models;
using PHAMVIETDUNG_SE1885_A01_BE.DataAccess.Repositories;

namespace PHAMVIETDUNG_SE1885_A01_BE.BusinessLogic.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repository;
        private readonly IGenericRepository<NewsTag> _newsTagRepo;

        public TagService(ITagRepository repository, IGenericRepository<NewsTag> newsTagRepo)
        {
            _repository = repository;
            _newsTagRepo = newsTagRepo;
        }

        public IEnumerable<Tag> GetAllTags()
        {
            return _repository.GetAll();
        }

        public Tag GetTagById(int id)
        {
            return _repository.GetById(id);
        }

        public void CreateTag(Tag tag)
        {
            if (_repository.GetAll().Any(t => t.TagName.ToLower() == tag.TagName.ToLower()))
            {
                throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateTag));
            }
            tag.TagId = _repository.GetMaxTagId() + 1;
            _repository.Insert(tag);
        }

        public void UpdateTag(Tag tag)
        {
            var existing = _repository.GetById(tag.TagId);
            if (existing != null)
            {
                if (!existing.TagName.Equals(tag.TagName, StringComparison.OrdinalIgnoreCase))
                {
                     if (_repository.GetAll().Any(t => t.TagName.ToLower() == tag.TagName.ToLower()))
                    {
                        throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.DuplicateTag));
                    }
                }
                existing.TagName = tag.TagName;
                existing.Note = tag.Note;
                _repository.Update(existing);
            }
        }

        public void DeleteTag(int id)
        {
             // Rule: "A tag cannot be deleted if it is referenced in NewsTag"
            if (_newsTagRepo.GetAll().Any(nt => nt.TagId == id))
            {
                throw new Exception(Common.SystemMessages.GetMessage(Common.SystemMessages.UsedTagDeleteError));
            }

            _repository.Delete(id);
        }
    }
}
