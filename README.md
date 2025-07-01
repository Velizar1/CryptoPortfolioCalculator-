# CryptoPortfolioCalculator-
Crypto portfolio calculation.  
The project contains of a server and front-end side.

## Server side
In order to use the project, start the backend server.  A swagger supported page with endpoin documentation should appear.    Retrieving calculated crypto coin profits base on uploaded portfolio file with format of each line :  
number of coins(number)|coin(code)|initial buy price(number)  
Log files with operations steps and errors will be generated in ./CryptoPortfolio/Logs    

## Client React App
You can find the react application settled in ./React.UI folder. Open this folder with VS code and add .env file with the following content :  

REACT_APP_API_BASE = https://localhost:7225/Portfolio   -- base address, keep in mind that the port must the same as the one in server config sile  
REACT_APP_API_UPLOAD = /UploadCryptoPortfolio           -- upload endpoint  
REACT_APP_API_REFRESH = /RefreshInformation             -- refresh endpoint  
REACT_APP_POLL_INTERVAL = 60000                         -- refres polling intereval in ms  
REACT_APP_OPENAI_API_KEY = XXXX                         -- openai api key - https://platform.openai.com/api-keys  

In the project directory, you can run:  

### `npm start`

Runs the app in the development mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in the browser.

The page will reload if you make edits.\
You will also see any lint errors in the console.

## Learn More

You can learn more in the [Create React App documentation](https://facebook.github.io/create-react-app/docs/getting-started).

To learn React, check out the [React documentation](https://reactjs.org/).
