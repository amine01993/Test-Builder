
export interface Test {
  Id: number;
  Name: string;
}

export interface Page {
  Id: number;
  Limit: number | null;
  Position: number;
  Name: string;
  Shuffle: boolean;
  TestId: number;
}

export interface TestQuestion {
  Id: number;
  Position: number;
  Random: boolean;
  QuestionId: number | null;
  QuestionIds: string | null;
  Number: number | null;

  TypeId: number | null;
  TypeName: string | null;
  Question: string | null;
  Selection: number | null;
  Answers: Answer[] | null;
}

export interface Answer {
  Id: number;
  _Answer: string;
  Correct: boolean;
  Match: string | null;
  Points: number | null;
  Penalty: number | null;
}
