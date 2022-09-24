import time
import orjson
# same interface as requests library (but Client insteased of session)
import httpx as requests

DESTINATION = 'http://127.0.0.1'
PORT = 50000
ENDPOINT = 'get_content'

URL = f'{DESTINATION}:{PORT}/{ENDPOINT}'

print(orjson.loads(requests.get(URL).text))

total_per_test = list()
total_rpses = list()
TEST_TIME = 20  # sec
TESTS_COUNT = 6
WITH_SERIALIZING = True


for _ in range(TESTS_COUNT):
    test_start = time.time()

    while time.time() - test_start < TEST_TIME:

        client = requests.Client()

        execute_time = time.time()
        if WITH_SERIALIZING:
            orjson.loads(client.get(URL).text)
        else:
            requests.get(URL)
        end_time = time.time()

        total_rpses.append(1 / (end_time - execute_time))

    total_per_test.append(sum(total_rpses) / len(total_rpses))
    total_rpses = list()

# avg RPSes: [701.3665034718299, 734.7376362747075, 732.967981700563, 729.3280870803726, 716.0553230085226, 714.9961458628015] for 6 times by 20 seconds of tests without serializing
# avg RPSes: [599.3323153431462, 538.1370351535841, 679.0107213927986, 666.6057288651386, 669.1316000597229, 673.6264615252892] for 6 times by 20 seconds of tests with serializing
print(
    f'avg RPSes: {total_per_test} for {TESTS_COUNT} times by {TEST_TIME} seconds of tests'
)
