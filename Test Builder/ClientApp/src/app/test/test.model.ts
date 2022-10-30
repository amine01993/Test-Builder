
export interface Test {
  Id: number;
  Name: string;
  Pages: Page[];
}

export interface Page {
  Id: number;
  Limit: number | null;
  Position: number;
  Name: string;
  Shuffle: boolean;
  TestId: number;
  PageQuestions: PageQuestion[];
}

export interface PageQuestion {
  Id: number;
  Position: number;
  Random: boolean;
  QuestionId: number | null;
  QuestionIds: string | null;
  Number: number | null;
  Question: Question | null;        
}

export interface Question {
  Id: number;
  TypeId: number;
  Selection: number | null;
  _Question: string;
  Answers: Answer[] | null;
  QuestionType: QuestionType;
}

export interface QuestionType {
  Id: number;
  Name: string;
}

export interface Answer {
  Id: number;
  _Answer: string;
  Correct: boolean;
  Match: string | null;
  Points: number | null;
  Penalty: number | null;
}
