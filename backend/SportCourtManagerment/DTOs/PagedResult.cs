namespace SportCourtManagerment.DTOs;

/// <summary>
/// Generic pagination wrapper matching the PagedResult&lt;T&gt; format
/// defined in API_Design.md §1.4.
/// </summary>
public class PagedResult<T>
{
  public List<T> Items { get; set; } = new();
  public int TotalCount { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
  public bool HasNextPage => PageNumber < TotalPages;
  public bool HasPreviousPage => PageNumber > 1;
}
