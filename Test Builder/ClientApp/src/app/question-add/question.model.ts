
export interface QuestionType {
  Id: number;
  Name: string;
  Icon: string;
  Link: string;
}

export interface DataResult<T> {
  Total: number;
  Count: number;
  Data: T[];
}

export interface QuestionResult {
  Id: number;
  Question: string;
  Points: number;
  CategoryName: string;
  SubCategoryName: string;
  QuestionTypeName: string;
}

export interface Question {
  Id: number;
  TypeId: number;
  CategoryId: number;
  Points: number;
  Penalty: number | null;
  Shuffle: number | null;
  Selection: number | null;
  _Question: string;
  Answers: Answer[] | null;
}

export interface Answer {
  Id: number;
  _Answer: string;
  Match: string | null;
  QuestionId: number;
  Correct: boolean;
  Points: number | null;
  Penalty: number | null;
}
