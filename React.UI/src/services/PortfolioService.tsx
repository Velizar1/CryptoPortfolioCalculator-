import { Config } from "../helpers/ConfigHelper";
import { CoinRow } from "../types/CoinRowType";
import OpenAI from "openai";
import type {
  ResponseInput,
  ResponseInputItem,
} from "openai/resources/responses/responses";

const client = new OpenAI({
  apiKey: Config.openAiKey,
  dangerouslyAllowBrowser: true,
});

const inputMessage: string =
  "Write a one-sentence information about the stability of the coin and is it a good time to buy it. Return only a single new line at the end of every line and no other in the response content.";

const buildMessage = (content: string): ResponseInputItem => ({
  role: "user",
  content,
});

export async function updateAiInfo(
  json: CoinRow[] | undefined
): Promise<CoinRow[]> {
  if (!json) return [];

  let rowsWithInfo: CoinRow[];

  if (!client.apiKey || client.apiKey === "") {
    rowsWithInfo = json.map((row, i) => ({
      ...row,
      coinInformation: "Not available",
    }));
  } else {
    const aiInput: ResponseInput = [
      {
        role: "developer",
        content: `${inputMessage}`,
      },
      ...json.map((row) => buildMessage(`Coin: ${row.coinCode}.`)),
    ];
    
    const aiRes = await client.responses.create({
      model: "gpt-4.1",
      input: aiInput,
      stream: false,
    });

    const summaries = aiRes.output_text
      .trim()
      .replace(/(?:\r?\n){2,}/g, "\n")
      .split("\n");

    rowsWithInfo = json.map((row, i) => ({
      ...row,
      coinInformation: summaries[i] ?? "",
    }));
  }

  return rowsWithInfo;
}

export async function refreshInformation(): Promise<CoinRow[]> {
  const res = await fetch(Config.api.refresh, {
    method: "GET",
    credentials: "include",
  });

  if (!res.ok) throw new Error(await res.text());
  const json: CoinRow[] = await res.json();
  return json;
}

export async function uploadFile(formData: FormData): Promise<Response> {
  return fetch(Config.api.upload, {
    method: "PUT",
    body: formData,
    credentials: "include",
  });
}
