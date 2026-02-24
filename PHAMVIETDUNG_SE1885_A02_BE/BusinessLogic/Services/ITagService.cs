using PHAMVIETDUNG_SE1885_A02_BE.DataAccess.Models;

namespace PHAMVIETDUNG_SE1885_A02_BE.BusinessLogic.Services
{
  public interface ITagService
  {
    IEnumerable<Tag> GetAllTags();
    Tag GetTagById(int id);
    void CreateTag(Tag tag);
    void UpdateTag(Tag tag);
    void DeleteTag(int id);
  }
}
