const baseURL = "/api/"
export interface URLType {
  url: string;
  whiteList?: Array<string>;
  keepCasing?: boolean;
}
export const URLS: { [type: string]: { [action: string]: URLType } } = {
  users: {
    AUTH: {
      url: "users/auth"
    },
    ADD: {
      url: "users/add"
    },
    CURRENT: {
      url: "users/current"
    },
    CHANGE_PASSWORD: {
      url: "users/changepassword"
    },
  }
};

export function URLF(type: string, action: string, urlParams = {} as { [key: string]: any }) {
  let urlEntry = { ...URLS[type][action] } as URLType;
  urlEntry.url = `${baseURL}${urlEntry.url}`.replace(/{(.*?)}/g, (_, m) => urlParams[m]);
  return urlEntry;
}
