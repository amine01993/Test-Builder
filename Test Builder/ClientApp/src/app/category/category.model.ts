
export interface Category {
  Id: number;
  Name: string;
  ParentId: number | null;
  Parent: Category | null;
}
