import { BASEURL, URLType } from "constants/URLS";
import { titleCase } from ".";

const fetching = async (url: URLType, queryParams: any, token: any, baseUrl?: string) => {
  const { url: urlName, whiteList: urlWhiteList = null, keepCasing = false } = url;
  let options: any = {
    method: "POST",
    headers: {},
  };
  if (queryParams) {
    if (queryParams.__proto__ !== FormData.prototype) {
      options.headers["Content-Type"] = "application/json";
      let whiteListedParams = {};
      if (!urlWhiteList) {
        whiteListedParams = { ...queryParams };
      } else {
        urlWhiteList.forEach((field) => {
          if (field in queryParams) {
            whiteListedParams[field] = queryParams[field];
          }
        });
      }
      queryParams = JSON.stringify(keepCasing ? whiteListedParams : titleCase(whiteListedParams));
    }
    options.body = queryParams;
  }

  if (token) {
    options.headers["Authorization"] = token;
  }

  return await fetch(`${baseUrl || BASEURL}${urlName}`, options);
};

export default fetching;
